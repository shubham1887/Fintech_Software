using FintechApp.Entities;
using FintechApp.Requests;
using FintechApp.Requests.User;

namespace FintechApp.Interfaces.Repositories
{
    public interface IUserRepository
    {
        // Full list cache - Main method
        Task<List<UserMaster>> GetAllUsers();
        /// <summary>All users INCLUDING soft-deleted rows (admin All-Users list with ShowDeleted).</summary>
        Task<List<UserMaster>> GetAllUsersWithDeleted();

        // Search from cached list
        Task<UserMaster?> GetUserById(int userId);
        Task<UserMaster?> GetUserByMobile(string mobileNumber);
        Task<UserMaster?> GetUserByEmail(string email);
        Task<List<UserMaster>> GetUsersByUserType(int userTypeId);
        Task<List<UserMaster>> GetUsersByParent(int parentId);

        // Filtered list from cache
        Task<List<UserMaster>> GetUsers(GetUserRequest request);

        // Update operations (clear cache)
        Task<bool> CreateUser(UserMaster user);
        Task<bool> UpdateUser(UserMaster user);
        Task<bool> DeleteUser(int userId);
        Task<bool> RestoreUser(int userId);

        // Clear cache manually
        void ClearUserCache();

        /// <summary>
        /// End every live session for this user by moving their session generation on by one.
        /// Signing out calls it, so the access token stops working everywhere instead of
        /// outliving the sign-out in whatever tab or device still holds a copy.
        /// </summary>
        Task<bool> BumpSessionEpoch(int userId);

        // Login-specific
        Task<UserMaster?> GetUserByMobileWithPassword(string mobile, string passwordHash);
        Task<UserMaster?> GetUserByIdWithPassword(int userId, string passwordHash);

        /// <summary>
        /// Switch the two-step-verification flag the login path gates on.
        ///
        /// Narrow on purpose. The cached UserMaster instances handed out by GetUserById are
        /// detached, so passing one to UpdateUser would write every column back from a snapshot
        /// that may be minutes old — flipping one flag would quietly revert whatever else had
        /// changed in the meantime. This loads the tracked row and touches only the flag.
        /// </summary>
        Task<bool> SetTwoFactorEnabled(int userId, bool enabled);
    }
}
