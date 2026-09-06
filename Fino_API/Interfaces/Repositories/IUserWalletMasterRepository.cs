using FintechApp.Entities;
using FintechApp.StaticModels;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserWalletMasterRepository
    {
        Task<List<UserWalletMaster>> GetAll();
        Task<List<UserWalletMaster>> GetByUserID(int userId);
        Task<UserWalletMaster?> GetByID(int id);

        /// <summary>Fresh single-row lookup for one user's wallet (no cache).</summary>
        Task<UserWalletMaster?> GetByUserAndWallet(int userId, int walletId);

        /// <summary>
        /// Atomically add <paramref name="delta"/> to a wallet balance with a single conditional
        /// UPDATE (database row-lock — safe under concurrency and across app instances, no queue).
        /// For a debit (negative delta) the change is rejected unless the wallet can afford it:
        ///   - allowOverdraft: the admin float account may go negative.
        ///   - respectHold: a non-admin debit may not eat into HoldBalance (frozen portion).
        /// Returns the pre/post balance the DB saw, or Applied=false when rejected/row missing.
        /// </summary>
        Task<WalletDeltaResult> TryApplyDelta(int userId, int walletId, decimal delta, bool allowOverdraft, bool respectHold);

        /// <summary>Atomically add <paramref name="holdDelta"/> to HoldBalance only (never touches
        /// Balance), so an admin hold cannot clobber a concurrent debit/credit. Returns rows affected.</summary>
        Task<int> ApplyHoldDelta(int userId, int walletId, decimal holdDelta);

        Task<int> Insert(UserWalletMaster entity);
        Task<int> Update(UserWalletMaster entity);
        Task<int> Delete(int id);
    }
}
