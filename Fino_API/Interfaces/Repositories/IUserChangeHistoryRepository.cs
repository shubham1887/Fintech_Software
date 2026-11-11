using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IUserChangeHistoryRepository
    {
        Task<int> Insert(UserChangeHistory entity);

        /// <summary>Append several rows for one operation (an edit that touched many fields).</summary>
        Task<int> InsertRange(IEnumerable<UserChangeHistory> entities);

        /// <summary>
        /// Filtered, paged change history, newest first, plus the total the filter matched.
        /// <paramref name="allowedUserIds"/> null = unrestricted (admin); otherwise the rows are
        /// clipped to changes made to those users.
        /// </summary>
        Task<(int count, List<UserChangeHistory> list)> Search(
            int userId, int changedByUserId, DateTime? fromDate, DateTime? toDate, string action,
            IEnumerable<int>? allowedUserIds, int skip, int take);
    }
}
