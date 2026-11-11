using System.Data;
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FintechApp.Repositories
{
    /// <summary>
    /// Outstanding balances (one row per user). Reads of the whole table are cached for the report
    /// screens; the BALANCE itself is only ever changed through <see cref="TryApplyDelta"/>, a
    /// single atomic conditional UPDATE, so two concurrent credit approvals cannot both squeeze
    /// past the same credit limit.
    /// </summary>
    public class CashCollectionMasterRepository : ICashCollectionMasterRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<CashCollectionMaster>> _listCache;
        public CashCollectionMasterRepository(ApplicationDbContext context) { _context = context; _listCache = new MemoryCacheProcessing<List<CashCollectionMaster>>(); }
        public async Task<List<CashCollectionMaster>> GetAll() => _listCache.GetOrCreate(CacheKeys.AllCashCollectionMasters, () => _context.CashCollectionMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList()) ?? new List<CashCollectionMaster>();
        public async Task<CashCollectionMaster?> GetByID(int id) { var list = await GetAll(); return list.FirstOrDefault(x => x.CashCollectionMasterID == id); }
        public async Task<List<CashCollectionMaster>> GetByUserID(int userid) { var list = await GetAll(); return list.Where(x => x.UserID == userid).ToList(); }
        public async Task<int> Insert(CashCollectionMaster entity) { await _context.CashCollectionMasters.AddAsync(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashCollectionMasters); return result; }
        public async Task<int> Update(CashCollectionMaster entity) { _context.CashCollectionMasters.Update(entity); int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashCollectionMasters); return result; }
        public async Task<int> Delete(int id) { var entity = await _context.CashCollectionMasters.FindAsync(id); if (entity == null) return 0; entity.IsDeleted = true; entity.UpdatedateTime = DateTime.UtcNow; int result = await _context.SaveChangesAsync(); if (result > 0) _listCache.Remove(CacheKeys.AllCashCollectionMasters); return result; }

        public async Task<CashCollectionMaster?> GetLiveByUserID(int userID)
        {
            return await _context.CashCollectionMasters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserID == userID && !x.IsDeleted);
        }

        public async Task<CashCollectionMaster?> EnsureRow(int userID, decimal defaultMaxOutstanding = 0m)
        {
            var existing = await GetLiveByUserID(userID);
            if (existing != null) return existing;

            try
            {
                await Insert(new CashCollectionMaster
                {
                    UserID = userID,
                    Outstanding = 0m,
                    MaxOutstanding = defaultMaxOutstanding,
                    DateTime = DateTime.UtcNow,
                    UpdatedateTime = DateTime.UtcNow,
                });
            }
            catch (DbUpdateException)
            {
                // Two requests for the same brand-new user raced to create the row. The loser does
                // not care which one won — it only needs the row to exist, and now it does.
                _listCache.Remove(CacheKeys.AllCashCollectionMasters);
            }

            return await GetLiveByUserID(userID);
        }

        public async Task<int> SetMaxOutstanding(int userID, decimal maxOutstanding, bool hasExplicitLimit)
        {
            int updated = await _context.CashCollectionMasters
                .Where(x => x.UserID == userID && !x.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.MaxOutstanding, maxOutstanding)
                    .SetProperty(x => x.HasExplicitLimit, hasExplicitLimit)
                    .SetProperty(x => x.UpdatedateTime, DateTime.UtcNow));

            if (updated > 0) _listCache.Remove(CacheKeys.AllCashCollectionMasters);
            return updated;
        }

        public async Task<OutstandingDeltaResult> TryApplyDelta(int userID, decimal delta, bool enforceLimit, decimal effectiveLimit)
        {
            // One statement: test both guards and apply the change under the row lock, returning
            // the exact before/after via OUTPUT so the statement row written next is correct
            // without a second read that another request could have moved underneath us.
            //
            //   credit  (delta > 0): refused when Outstanding + delta > MaxOutstanding
            //   payment (delta < 0): refused when Outstanding + delta < 0
            //
            // MaxOutstanding = 0 means "no credit allowed", not "unlimited" — that is the same
            // meaning the payment-mode list already gives it when it hides the Credit option, and
            // it is the safe reading: a brand-new user with no limit set can take nothing on credit.
            const decimal eps = -0.0001m;
            const string sql =
                "UPDATE CashCollectionMasters " +
                "SET Outstanding = Outstanding + @delta, UpdatedateTime = SYSUTCDATETIME() " +
                "OUTPUT deleted.Outstanding AS OpeningBalance, inserted.Outstanding AS ClosingBalance " +
                "WHERE UserID = @userId AND IsDeleted = 0 " +
                "AND (Outstanding + @delta) >= @eps " +
                "AND (@enforceLimit = 0 OR @delta <= 0 OR (Outstanding + @delta) <= @effectiveLimit)";

            var conn = _context.Database.GetDbConnection();
            IDbContextTransaction? current = _context.Database.CurrentTransaction;

            bool openedHere = false;
            if (conn.State != ConnectionState.Open)
            {
                await _context.Database.OpenConnectionAsync();
                openedHere = true;
            }

            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                if (current != null) cmd.Transaction = current.GetDbTransaction();

                AddParam(cmd, "@delta", delta);
                AddParam(cmd, "@userId", userID);
                AddParam(cmd, "@enforceLimit", enforceLimit ? 1 : 0);
                AddParam(cmd, "@effectiveLimit", effectiveLimit);
                AddParam(cmd, "@eps", eps);

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        // The limit reported back is the one actually enforced, which may be the
                        // Key Setting default rather than the row's own column.
                        var applied = OutstandingDeltaResult.Ok(
                            reader.GetDecimal(0), reader.GetDecimal(1), effectiveLimit);
                        // The cached table now disagrees with the database, and the report screens
                        // read it. Dropping it here rather than in the service means no caller can
                        // forget to.
                        _listCache.Remove(CacheKeys.AllCashCollectionMasters);
                        return applied;
                    }
                }
            }
            finally
            {
                // Leave an EF-owned connection open (the scope disposes it); only close what we
                // opened, and only when EF is not mid-transaction on it.
                if (openedHere && current == null)
                    await _context.Database.CloseConnectionAsync();
            }

            // Nothing was updated, so classify it for the message the person will read. This read
            // is deliberately AFTER the attempt and only on the refusal path: the balance it
            // reports is for explaining, never for deciding.
            var row = await GetLiveByUserID(userID);
            if (row == null) return OutstandingDeltaResult.Missing;
            if (delta < 0) return OutstandingDeltaResult.Negative(row.Outstanding, effectiveLimit);
            return OutstandingDeltaResult.OverLimit(row.Outstanding, effectiveLimit);
        }

        private static void AddParam(System.Data.Common.DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }
    }
}
