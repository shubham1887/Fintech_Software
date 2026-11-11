using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ILoginHistoryRepository
    {
        Task<List<LoginHistory>> GetByUserID(int userId, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize);

        // Every filter applied in the query, before paging — including the channel and the set of
        // users the caller is allowed to see. Pass allowedUserIds = null for an unrestricted caller.
        Task<List<LoginHistory>> GetFiltered(
            int userId, DateTime? fromDate, DateTime? toDate, string through, IEnumerable<int>? allowedUserIds,
            int pageNumber, int pageSize);

        Task<int> Insert(LoginHistory entity);
    }
}
