using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IWalletHoldHistoryRepository
    {
        Task<int> Insert(WalletHoldHistory entity);

        // Filtered, paged hold history plus the total the filter matched.
        Task<(int count, List<WalletHoldHistory> list)> Search(
            int userId, int walletId, DateTime? fromDate, DateTime? toDate, string action,
            IEnumerable<int>? allowedUserIds, int skip, int take);
    }
}
