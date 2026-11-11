using FintechApp.Data;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using Hangfire;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Jobs
{
    /// <summary>
    /// The end-of-day close, run inside the nightly maintenance window (23:50 by default, which is
    /// what the System Down window in Key Settings is set to).
    ///
    /// Three things happen, each on its own and each logged on its own, so one failing never stops
    /// the others:
    ///   1. A COPY_ONLY backup of the database to <c>NightlySnapshot:BackupFolder</c>, with files
    ///      older than <c>KeepDays</c> removed.
    ///   2. Every active provider is asked for its live balance (the same call as the dashboard's
    ///      refresh button) so the stock figure written is the provider's own, not the last reply.
    ///   3. A snapshot row per user wallet and per provider is written to DailyBalanceSnapshots.
    ///      That is tonight's CLOSING and tomorrow's OPENING — the figure the dashboard's opening
    ///      balance was never able to state reliably from the ledger alone.
    ///
    /// The snapshot table is created by the job itself if the install predates it (the same DDL is
    /// in Scripts/install), so an upgrade needs no manual SQL for this to start working.
    /// </summary>
    public class DailySnapshotJob
    {
        public const string JobId = "nightly-snapshot";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DailySnapshotJob> _log;

        public DailySnapshotJob(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<DailySnapshotJob> log)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _log = log;
        }

        private const string EnsureTableSql = @"
IF OBJECT_ID(N'[DailyBalanceSnapshots]', N'U') IS NULL
BEGIN
CREATE TABLE [DailyBalanceSnapshots] (
    [DailyBalanceSnapshotID] int NOT NULL IDENTITY,
    [SnapshotDate] date NOT NULL,
    [Kind] varchar(10) NOT NULL,
    [OwnerID] int NOT NULL,
    [WalletID] int NOT NULL,
    [Balance] decimal(18,5) NOT NULL,
    [HoldBalance] decimal(18,5) NOT NULL,
    [TakenAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DailyBalanceSnapshots] PRIMARY KEY ([DailyBalanceSnapshotID])
);
CREATE UNIQUE INDEX [UX_DailyBalanceSnapshots_Day] ON [DailyBalanceSnapshots] ([SnapshotDate], [Kind], [OwnerID], [WalletID]);
END;";

        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        public async Task Run()
        {
            var section = _configuration.GetSection("NightlySnapshot");
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;

            // 1. Backup.
            try
            {
                await BackupDatabase(section.GetValue("BackupFolder", @"C:\Backups\finocore-db") ?? @"C:\Backups\finocore-db",
                                     section.GetValue("KeepDays", 7));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "nightly snapshot: the database backup FAILED.");
            }

            // 2. Ask every provider for its balance so the stock written below is theirs.
            try
            {
                var gateway = sp.GetRequiredService<IProviderGatewayService>();
                var (fail, results) = await gateway.CheckAllBalances();
                int answered = results.Count(r => string.Equals(r.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase));
                _log.LogInformation("nightly snapshot: provider balances enquired — {Answered}/{Total} answered. {Fail}",
                    answered, results.Count, fail);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "nightly snapshot: provider balance enquiry FAILED; last known balances will be snapshotted.");
            }

            // 3. The closing figures.
            try
            {
                var db = sp.GetRequiredService<ApplicationDbContext>();
                await db.Database.ExecuteSqlRawAsync(EnsureTableSql);

                var wallets = await sp.GetRequiredService<IUserWalletMasterRepository>().GetAll();
                var providers = await sp.GetRequiredService<IProviderMasterRepository>().GetAll();
                DateTime now = DateTime.Now;
                // The snapshot is a day's CLOSING. Scheduled for 23:50, but if the app pool was
                // asleep overnight Hangfire fires the missed run on the first request next morning
                // (seen on live: 06:36 on 11 Nov). A run before noon therefore belongs to the day
                // that just ended, not to the one that has barely started.
                DateTime day = now.Hour < 12 ? now.Date.AddDays(-1) : now.Date;

                // Re-runnable: a second run on the same night replaces that night's rows.
                await db.Database.ExecuteSqlRawAsync("DELETE FROM [DailyBalanceSnapshots] WHERE [SnapshotDate] = {0}", day);

                var conn = db.Database.GetDbConnection();
                bool opened = conn.State != System.Data.ConnectionState.Open;
                if (opened) await conn.OpenAsync();
                try
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO [DailyBalanceSnapshots] ([SnapshotDate],[Kind],[OwnerID],[WalletID],[Balance],[HoldBalance],[TakenAt]) VALUES (@d,@k,@o,@w,@b,@h,@t)";
                    var pD = new SqlParameter("@d", System.Data.SqlDbType.Date) { Value = day };
                    var pK = new SqlParameter("@k", System.Data.SqlDbType.VarChar, 10);
                    var pO = new SqlParameter("@o", System.Data.SqlDbType.Int);
                    var pW = new SqlParameter("@w", System.Data.SqlDbType.Int);
                    var pB = new SqlParameter("@b", System.Data.SqlDbType.Decimal) { Precision = 18, Scale = 5 };
                    var pH = new SqlParameter("@h", System.Data.SqlDbType.Decimal) { Precision = 18, Scale = 5 };
                    var pT = new SqlParameter("@t", System.Data.SqlDbType.DateTime2) { Value = now };
                    foreach (var p in new[] { pD, pK, pO, pW, pB, pH, pT }) cmd.Parameters.Add(p);

                    int userRows = 0, providerRows = 0;
                    foreach (var w in wallets)
                    {
                        pK.Value = "USER"; pO.Value = w.UserID; pW.Value = w.WalletID; pB.Value = w.Balance; pH.Value = w.HoldBalance;
                        userRows += await cmd.ExecuteNonQueryAsync();
                    }
                    foreach (var p in providers.Where(p => !p.IsDelete))
                    {
                        // Wallet 1 = recharge stock, wallet 2 = DMT stock (WalletCode), so the two
                        // balances a provider carries land on the wallet they belong to.
                        pK.Value = "PROVIDER"; pO.Value = p.ProviderID; pH.Value = 0m;
                        pW.Value = (int)StaticModels.WalletCode.Recharge; pB.Value = p.RechargeBalance;
                        providerRows += await cmd.ExecuteNonQueryAsync();
                        pW.Value = (int)StaticModels.WalletCode.DMT; pB.Value = p.DMTBalance;
                        providerRows += await cmd.ExecuteNonQueryAsync();
                    }
                    _log.LogInformation("nightly snapshot: {Users} user-wallet rows and {Providers} provider rows written for {Day:yyyy-MM-dd}.",
                        userRows, providerRows, day);
                }
                finally
                {
                    if (opened) await conn.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "nightly snapshot: writing the balance snapshot FAILED.");
            }
        }

        /// <summary>
        /// BACKUP DATABASE runs inside SQL Server as its service account, which often cannot write
        /// to an arbitrary folder — so a permission failure retries into the instance's own backup
        /// folder, the one place it can always write. COMPRESSION is not on Express, so that is
        /// retried without too. (Mirrors deploy/lib/FinoCommon.ps1 Invoke-DatabaseBackup.)
        /// </summary>
        private async Task BackupDatabase(string folder, int keepDays)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            if (connectionString.Length == 0) { _log.LogWarning("nightly snapshot: no connection string, backup skipped."); return; }

            var builder = new SqlConnectionStringBuilder(connectionString);
            string database = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(database)) { _log.LogWarning("nightly snapshot: connection string names no database, backup skipped."); return; }

            try { Directory.CreateDirectory(folder); } catch { /* SQL Server may still be able to write it */ }
            string fileName = $"{database}-{DateTime.Now:yyyyMMdd-HHmm}.bak";
            string target = Path.Combine(folder, fileName);

            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            async Task Run(string path, bool compression)
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandTimeout = 0;
                cmd.CommandText = $"BACKUP DATABASE [{database}] TO DISK = @path WITH COPY_ONLY, INIT{(compression ? ", COMPRESSION" : string.Empty)}, NAME = @label;";
                cmd.Parameters.AddWithValue("@path", path);
                cmd.Parameters.AddWithValue("@label", "Fino nightly backup");
                await cmd.ExecuteNonQueryAsync();
            }

            string finalPath = target;
            try { await Run(target, compression: true); }
            catch (SqlException ex) when (ex.Message.Contains("COMPRESSION", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("not supported", StringComparison.OrdinalIgnoreCase))
            {
                await Run(target, compression: false);
            }
            catch (SqlException ex) when (ex.Message.Contains("Operating system error 5") || ex.Message.Contains("Access is denied") || ex.Message.Contains("Cannot open backup device"))
            {
                await using var pathCmd = conn.CreateCommand();
                pathCmd.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000))";
                string? defaultPath = (await pathCmd.ExecuteScalarAsync()) as string;
                if (string.IsNullOrWhiteSpace(defaultPath)) throw;
                finalPath = Path.Combine(defaultPath, fileName);
                try { await Run(finalPath, compression: true); } catch (SqlException) { await Run(finalPath, compression: false); }
            }

            _log.LogInformation("nightly snapshot: database {Db} backed up to {Path}.", database, finalPath);

            // Keep the last N days.
            try
            {
                string dir = Path.GetDirectoryName(finalPath) ?? folder;
                foreach (var f in new DirectoryInfo(dir).GetFiles($"{database}-*.bak"))
                {
                    if (f.LastWriteTime < DateTime.Now.AddDays(-Math.Max(1, keepDays))) f.Delete();
                }
            }
            catch (Exception ex) { _log.LogWarning(ex, "nightly snapshot: could not prune old backups."); }
        }
    }
}
