using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IUserQuickServiceRepository
    {
        Task<List<UserQuickService>> GetByUserID(int userId);

        /// <summary>
        /// Replace a user's entire shortcut list with <paramref name="serviceIds"/>, in that order.
        ///
        /// The list IS the setting, so it is saved as one — adding and removing rows individually
        /// would leave a half-saved set visible to the phone mid-edit, and would make "I removed
        /// the last one" impossible to express.
        /// </summary>
        Task<int> ReplaceForUser(int userId, IReadOnlyList<int> serviceIds);
    }
}
