using System.Data;
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FintechApp.Repositories
{
    // Wallet balances are money: they are the most write-hot, correctness-critical rows in the system.
    // They are deliberately NOT cached. An in-memory cache of balances handed out stale (and, worse,
    // shared-mutable) values that surfaced as spurious "insufficient balance" errors and lost updates
    // under load. Reads here hit the DB fresh (indexed by UserID) and balance changes go through a
    // single atomic conditional UPDATE (TryApplyDelta) so concurrency is handled by the database
    // row-lock — correct under 100+ concurrent users / hundreds of writes a minute, and across
    // multiple app instances, with no queue.
    public class UserWalletMasterRepository : IUserWalletMasterRepository
    {
        private readonly ApplicationDbContext _context;

        public UserWalletMasterRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserWalletMaster>> GetAll()
        {
            return await _context.UserWalletMasters.AsNoTracking()
                .Include(x => x.WalletMaster)
                .Include(x => x.UserMaster)
                .ToListAsync();
        }

        public async Task<List<UserWalletMaster>> GetByUserID(int userId)
        {
            return await _context.UserWalletMasters.AsNoTracking()
                .Where(x => x.UserID == userId)
                .ToListAsync();
        }

        public async Task<UserWalletMaster?> GetByID(int id)
        {
            return await _context.UserWalletMasters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserWalletMasterID == id);
        }

        public async Task<UserWalletMaster?> GetByUserAndWallet(int userId, int walletId)
        {
            return await _context.UserWalletMasters.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserID == userId && x.WalletID == walletId);
        }

        public async Task<WalletDeltaResult> TryApplyDelta(int userId, int walletId, decimal delta, bool allowOverdraft, bool respectHold)
        {
            // One atomic statement: check affordability and apply the delta under the DB row-lock, and
            // return the exact pre/post balance via OUTPUT so callers can write a correct ledger row
            // without a separate (racy) read. 0 rows back = rejected (row missing or unaffordable).
            //   spendable-after = Balance + delta - (respectHold ? HoldBalance : 0)
            //   allowed when     allowOverdraft OR spendable-after >= -eps
            const decimal eps = -0.0001m;
            const string sql =
                "UPDATE UserWalletMasters " +
                "SET Balance = Balance + @delta, BalanceHash = NULL " +
                "OUTPUT deleted.Balance AS OpeningBalance, inserted.Balance AS ClosingBalance " +
                "WHERE UserID = @userId AND WalletID = @walletId " +
                "AND (@allowOverdraft = 1 OR (Balance + @delta - (CASE WHEN @respectHold = 1 THEN HoldBalance ELSE 0 END)) >= @eps)";

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
                AddParam(cmd, "@userId", userId);
                AddParam(cmd, "@walletId", walletId);
                AddParam(cmd, "@allowOverdraft", allowOverdraft ? 1 : 0);
                AddParam(cmd, "@respectHold", respectHold ? 1 : 0);
                AddParam(cmd, "@eps", eps);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    decimal opening = reader.GetDecimal(0);
                    decimal closing = reader.GetDecimal(1);
                    return WalletDeltaResult.Ok(opening, closing);
                }
                return WalletDeltaResult.NotApplied;
            }
            finally
            {
                // Leave an EF-owned connection open (the scope disposes it); only close what we opened
                // and only when EF isn't mid-transaction on it.
                if (openedHere && current == null)
                    await _context.Database.CloseConnectionAsync();
            }
        }

        private static void AddParam(System.Data.Common.DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }

        public async Task<int> ApplyHoldDelta(int userId, int walletId, decimal holdDelta)
        {
            // Single UPDATE of HoldBalance only — leaves Balance (owned by TryApplyDelta) untouched, so
            // an admin placing/releasing a hold can never overwrite a concurrent debit/credit.
            return await _context.UserWalletMasters
                .Where(w => w.UserID == userId && w.WalletID == walletId)
                .ExecuteUpdateAsync(s => s.SetProperty(w => w.HoldBalance, w => w.HoldBalance + holdDelta));
        }

        public async Task<int> Insert(UserWalletMaster entity)
        {
            await _context.UserWalletMasters.AddAsync(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(UserWalletMaster entity)
        {
            // Balance changes should go through TryApplyDelta (atomic). This remains for non-balance
            // edits (e.g. HoldBalance, admin wallet maintenance). Load the tracked row and copy scalar
            // values so it is safe regardless of what the change tracker already holds.
            var tracked = await _context.UserWalletMasters.FindAsync(entity.UserWalletMasterID);
            if (tracked == null)
                _context.UserWalletMasters.Update(entity);
            else
                _context.Entry(tracked).CurrentValues.SetValues(entity);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(int id)
        {
            var entity = await _context.UserWalletMasters.FindAsync(id);
            if (entity == null) return 0;
            _context.UserWalletMasters.Remove(entity);
            return await _context.SaveChangesAsync();
        }
    }
}
