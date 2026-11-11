using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace FintechApp.Helpers
{
    /// <summary>
    /// Brings an empty (or an older) database up to the schema this build expects, at startup, so
    /// that installing the product for a new client is "set the connection string and run".
    ///
    /// Why this exists rather than <c>dotnet ef database update</c>: the EF model snapshot in this
    /// repo has drifted from the entities, so the migration chain alone does not produce a correct
    /// schema, and running EF tooling would also mean the .NET SDK has to be installed on the
    /// customer's server. Instead the schema ships as ordinary SQL in <c>Scripts/install</c>,
    /// applied in filename order and recorded in <c>__FinoInstallHistory</c>, so each script runs
    /// exactly once per database and a later release only applies what it adds.
    ///
    /// Safe to run against a database that is already current: the first thing it does is compare
    /// recorded hashes and skip everything already applied.
    /// </summary>
    public static class DatabaseBootstrapper
    {
        private const string HistoryTable = "__FinoInstallHistory";
        private const string AppLockName = "Fino.DatabaseBootstrapper";

        public static async Task RunAsync(IConfiguration config, ILogger logger, string contentRootPath)
        {
            if (!config.GetValue("Database:AutoSetup", false))
            {
                logger.LogInformation("Database bootstrap skipped (Database:AutoSetup is false).");
                return;
            }

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

            var scriptsPath = Path.Combine(contentRootPath, "Scripts", "install");
            if (!Directory.Exists(scriptsPath))
            {
                logger.LogWarning("Database bootstrap: {Path} does not exist; nothing to apply.", scriptsPath);
                return;
            }

            var sw = Stopwatch.StartNew();
            await EnsureDatabaseExistsAsync(connectionString, logger);

            // One installer at a time. Two instances starting together (a web farm, or a container
            // scaled before the first boot finished) would otherwise run the same CREATE TABLE
            // concurrently. The lock is held for the whole run and released when this connection closes.
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
            await AcquireInstallLockAsync(conn, logger);

            await EnsureHistoryTableAsync(conn);
            var applied = await LoadAppliedAsync(conn);

            var files = Directory.GetFiles(scriptsPath, "*.sql")
                                 .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                                 .ToList();

            var ran = 0;
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                var sql = await File.ReadAllTextAsync(file);
                var hash = Sha256(sql);

                if (applied.TryGetValue(name, out var previousHash))
                {
                    if (!string.Equals(previousHash, hash, StringComparison.OrdinalIgnoreCase))
                    {
                        // An already-applied script was edited. Re-running it could duplicate seed
                        // rows; ignoring it silently would hide a real packaging mistake.
                        logger.LogWarning(
                            "Database bootstrap: {Script} has changed since it was applied to this database. " +
                            "It was NOT re-run. Ship changes as a new, higher-numbered script instead.", name);
                    }
                    continue;
                }

                logger.LogInformation("Database bootstrap: applying {Script}...", name);
                var scriptTimer = Stopwatch.StartNew();
                try
                {
                    foreach (var batch in SplitOnGo(sql))
                    {
                        await using var cmd = conn.CreateCommand();
                        cmd.CommandText = batch;
                        cmd.CommandTimeout = 0; // the schema script is large
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Database bootstrap FAILED on {Script}. The application will not start.", name);
                    throw new InvalidOperationException($"Database setup failed while applying '{name}': {ex.Message}", ex);
                }

                scriptTimer.Stop();
                await RecordAppliedAsync(conn, name, hash, scriptTimer.ElapsedMilliseconds);
                ran++;
                logger.LogInformation("Database bootstrap: {Script} applied in {Ms} ms.", name, scriptTimer.ElapsedMilliseconds);
            }

            await SeedFirstAdminAsync(conn, config, logger);

            sw.Stop();
            logger.LogInformation(
                "Database bootstrap complete: {Ran} script(s) applied, {Skipped} already current, {Ms} ms total.",
                ran, files.Count - ran, sw.ElapsedMilliseconds);
        }

        /// <summary>Creates the target database if it is not there yet, by connecting to master.</summary>
        private static async Task EnsureDatabaseExistsAsync(string connectionString, ILogger logger)
        {
            var target = new SqlConnectionStringBuilder(connectionString);
            var databaseName = target.InitialCatalog;
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new InvalidOperationException("The connection string has no Database/Initial Catalog.");

            var master = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = "master" };

            await using var conn = new SqlConnection(master.ConnectionString);
            await conn.OpenAsync();

            await using var check = conn.CreateCommand();
            check.CommandText = "SELECT database_id FROM sys.databases WHERE name = @name";
            check.Parameters.AddWithValue("@name", databaseName);
            if (await check.ExecuteScalarAsync() is not null and not DBNull) return;

            logger.LogInformation("Database bootstrap: creating database [{Database}]...", databaseName);
            await using var create = conn.CreateCommand();
            // The name comes from our own configuration, not from a request, but it is still
            // bracket-quoted because CREATE DATABASE cannot take a parameter.
            create.CommandText = $"CREATE DATABASE [{databaseName.Replace("]", "]]")}]";
            create.CommandTimeout = 0;
            await create.ExecuteNonQueryAsync();
        }

        private static async Task AcquireInstallLockAsync(SqlConnection conn, ILogger logger)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_getapplock";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Resource", AppLockName);
            cmd.Parameters.AddWithValue("@LockMode", "Exclusive");
            cmd.Parameters.AddWithValue("@LockOwner", "Session");
            cmd.Parameters.AddWithValue("@LockTimeout", 600000); // 10 minutes
            var rc = cmd.Parameters.Add("@ReturnValue", System.Data.SqlDbType.Int);
            rc.Direction = System.Data.ParameterDirection.ReturnValue;
            cmd.CommandTimeout = 0;
            await cmd.ExecuteNonQueryAsync();

            var result = (int)(rc.Value ?? -1);
            if (result < 0)
                throw new InvalidOperationException(
                    $"Could not acquire the database install lock (sp_getapplock returned {result}). " +
                    "Another instance is probably still setting the database up.");

            logger.LogDebug("Database bootstrap: install lock acquired ({Result}).", result);
        }

        private static async Task EnsureHistoryTableAsync(SqlConnection conn)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
IF OBJECT_ID(N'[{HistoryTable}]', N'U') IS NULL
BEGIN
    CREATE TABLE [{HistoryTable}] (
        [ScriptName]  nvarchar(260) NOT NULL,
        [Sha256]      char(64)      NOT NULL,
        [AppliedOn]   datetime2     NOT NULL CONSTRAINT [DF_{HistoryTable}_AppliedOn] DEFAULT (SYSUTCDATETIME()),
        [DurationMs]  bigint        NOT NULL,
        [AppliedBy]   nvarchar(128) NOT NULL CONSTRAINT [DF_{HistoryTable}_AppliedBy] DEFAULT (SUSER_SNAME()),
        CONSTRAINT [PK_{HistoryTable}] PRIMARY KEY ([ScriptName])
    );
END;";
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task<Dictionary<string, string>> LoadAppliedAsync(SqlConnection conn)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT [ScriptName], [Sha256] FROM [{HistoryTable}]";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                map[reader.GetString(0)] = reader.GetString(1);
            return map;
        }

        private static async Task RecordAppliedAsync(SqlConnection conn, string name, string hash, long ms)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"INSERT INTO [{HistoryTable}] ([ScriptName],[Sha256],[DurationMs]) VALUES (@n,@h,@d)";
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@h", hash);
            cmd.Parameters.AddWithValue("@d", ms);
            await cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Creates the five user types and the first administrator when the database has no admin
        /// yet, using the mobile/password the installer collected. Never touches an existing admin,
        /// so a restart cannot reset a live system's password back to the installation default.
        /// </summary>
        private static async Task SeedFirstAdminAsync(SqlConnection conn, IConfiguration config, ILogger logger)
        {
            if (!config.GetValue("Database:SeedAdmin", true)) return;

            var mobile = config["Bootstrap:AdminMobile"];
            var password = config["Bootstrap:AdminPassword"];
            if (string.IsNullOrWhiteSpace(mobile) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogInformation("Database bootstrap: no Bootstrap:AdminMobile/AdminPassword configured; admin seed skipped.");
                return;
            }

            await using var seed = conn.CreateCommand();
            seed.CommandTimeout = 0;

            // The INSERT is composed from sys.columns rather than written out, because UserMasters
            // has ~50 NOT NULL columns with no default and gains more over time — a hand-written
            // column list silently rots into "cannot insert NULL" the next time an entity changes.
            // Every required column the caller does not set gets the empty value for its type.
            seed.CommandText = @"
SET NOCOUNT ON;

-- User types. The ids are referenced by role checks in code, so they are inserted explicitly.
IF NOT EXISTS (SELECT 1 FROM UserTypeMasters)
BEGIN
    SET IDENTITY_INSERT UserTypeMasters ON;
    INSERT INTO UserTypeMasters (UserTypeID, UserType, AliasName, IsActive, [DateTime], UpdatedateTime, IsDeleted) VALUES
     (1,'Admin','Admin',1,GETDATE(),GETDATE(),0),
     (2,'Super Distributor','SuperDist',1,GETDATE(),GETDATE(),0),
     (3,'Distributor','Distrib',1,GETDATE(),GETDATE(),0),
     (4,'Retailer','Retailer',1,GETDATE(),GETDATE(),0),
     (5,'API User','ApiUser',1,GETDATE(),GETDATE(),0);
    SET IDENTITY_INSERT UserTypeMasters OFF;
END;

IF EXISTS (SELECT 1 FROM UserMasters WHERE UserTypeID = 1 AND IsDeleted = 0)
BEGIN
    SELECT 0 AS Created;
END
ELSE
BEGIN
    DECLARE @cols nvarchar(max) = N'', @vals nvarchar(max) = N'';

    SELECT @cols = @cols + CASE WHEN @cols = N'' THEN N'' ELSE N', ' END + QUOTENAME(c.name),
           @vals = @vals + CASE WHEN @vals = N'' THEN N'' ELSE N', ' END +
                CASE c.name
                    WHEN 'UserTypeID'        THEN N'1'
                    WHEN 'PersonName'        THEN N'@personName'
                    WHEN 'FirmName'          THEN N'@firmName'
                    WHEN 'MobileNumber'      THEN N'@mobile'
                    WHEN 'PasswordHash'      THEN N'@hash'
                    WHEN 'EmailID'           THEN N'@email'
                    WHEN 'UniqueReferenceNo' THEN N'''ADMIN001'''
                    WHEN 'RegistrationType'  THEN N'''WEB'''
                    WHEN 'IsActive'          THEN N'1'
                    WHEN 'IsKYCApproved'     THEN N'1'
                    -- Forces the change-password prompt on the very first login.
                    WHEN 'IsInitialPassword' THEN N'1'
                    ELSE
                        CASE
                            WHEN t.name IN ('varchar','nvarchar','char','nchar','text','ntext') THEN N''''''
                            WHEN t.name IN ('int','bigint','smallint','tinyint','bit','decimal','numeric','float','real','money','smallmoney') THEN N'0'
                            WHEN t.name IN ('datetime','datetime2','date','smalldatetime','datetimeoffset') THEN N'GETDATE()'
                            WHEN t.name = 'uniqueidentifier' THEN N'NEWID()'
                            WHEN t.name IN ('varbinary','binary') THEN N'0x'
                            ELSE N'NULL'
                        END
                END
    FROM sys.columns c
    JOIN sys.types   t ON t.user_type_id = c.user_type_id
    WHERE c.object_id = OBJECT_ID(N'[UserMasters]')
      AND c.is_identity = 0
      AND c.is_computed = 0
      AND c.is_nullable = 0
      AND c.default_object_id = 0
    ORDER BY c.column_id;

    DECLARE @sql nvarchar(max) =
        N'INSERT INTO [UserMasters] (' + @cols + N') VALUES (' + @vals + N');';

    EXEC sp_executesql @sql,
        N'@personName varchar(200), @firmName varchar(200), @mobile varchar(20), @hash varchar(200), @email varchar(200)',
        @personName = @personName, @firmName = @firmName, @mobile = @mobile, @hash = @hash, @email = @email;

    SELECT 1 AS Created;
END;";

            seed.Parameters.AddWithValue("@personName", config["Bootstrap:AdminName"] ?? "Administrator");
            seed.Parameters.AddWithValue("@firmName", config["Bootstrap:BrandName"] ?? "Administrator");
            seed.Parameters.AddWithValue("@mobile", mobile);
            seed.Parameters.AddWithValue("@hash", Sha256(password));
            seed.Parameters.AddWithValue("@email", config["Bootstrap:AdminEmail"] ?? "");

            var created = Convert.ToInt32(await seed.ExecuteScalarAsync() ?? 0) == 1;
            logger.LogInformation(created
                ? $"Database bootstrap: administrator created for mobile {mobile}. Change this password after the first login."
                : "Database bootstrap: an administrator already exists; admin seed skipped.");
        }

        /// <summary>
        /// Splits a script on its GO batch separators. GO is a client-side keyword — SQL Server
        /// itself rejects it — so scripts written for SSMS/sqlcmd have to be cut up before they can
        /// be sent over ADO.NET. Separators inside a block comment are left alone.
        /// </summary>
        internal static IEnumerable<string> SplitOnGo(string sql)
        {
            var batch = new StringBuilder();
            var inBlockComment = false;

            foreach (var rawLine in sql.Split('\n'))
            {
                var line = rawLine.TrimEnd('\r');

                if (!inBlockComment && IsGoSeparator(line.Trim()))
                {
                    if (batch.ToString().Trim().Length > 0) yield return batch.ToString();
                    batch.Clear();
                    continue;
                }

                TrackBlockComment(line, ref inBlockComment);
                batch.AppendLine(line);
            }

            if (batch.ToString().Trim().Length > 0) yield return batch.ToString();
        }

        private static bool IsGoSeparator(string trimmedLine)
        {
            if (trimmedLine.Length < 2) return false;
            if (!trimmedLine.StartsWith("GO", StringComparison.OrdinalIgnoreCase)) return false;

            var rest = trimmedLine[2..].Trim();
            if (rest.Length == 0) return true;
            if (rest.StartsWith("--")) return true;      // "GO -- comment"
            return int.TryParse(rest, out _);            // "GO 5" (repeat count)
        }

        private static void TrackBlockComment(string line, ref bool inBlockComment)
        {
            for (var i = 0; i < line.Length - 1; i++)
            {
                if (!inBlockComment && line[i] == '-' && line[i + 1] == '-') return; // rest of the line is a comment
                if (!inBlockComment && line[i] == '/' && line[i + 1] == '*') { inBlockComment = true; i++; }
                else if (inBlockComment && line[i] == '*' && line[i + 1] == '/') { inBlockComment = false; i++; }
            }
        }

        private static string Sha256(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
