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

        // Login-specific
        Task<UserMaster?> GetUserByMobileWithPassword(string mobile, string passwordHash);
        Task<UserMaster?> GetUserByIdWithPassword(int userId, string passwordHash);
    }
}
