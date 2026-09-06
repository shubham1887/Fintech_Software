using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserMaster>> _userListCache;

        public AuthRepository(ApplicationDbContext context)
        {
            _context = context;
            _userListCache = new MemoryCacheProcessing<List<UserMaster>>();
        }

        /// <summary>
        /// Get ALL users from cache - Puri list ek saath
        /// </summary>
        private async Task<List<UserMaster>> GetAllUsers()
        {
            return _userListCache.GetOrCreate(CacheKeys.AllUsers, () =>
            {
                return _context.UserMasters
                    .Include(u => u.UserType)
                    .Include(u => u.State)
                    .Where(u => !u.IsDeleted)
                    .AsNoTracking()
                    .ToList();
            }) ?? new List<UserMaster>();
        }

        /// <summary>
        /// Get user by mobile number - Cached list se search
        /// </summary>
        public async Task<UserMaster?> GetUserByMobileNumber(string mobileNumber)
        {
            var allUsers = await GetAllUsers();
            return allUsers.FirstOrDefault(u => u.MobileNumber == mobileNumber);
        }

        /// <summary>
        /// Get user by ID - Cached list se search
        /// </summary>
        public async Task<UserMaster?> GetUserById(int userId)
        {
            var allUsers = await GetAllUsers();
            return allUsers.FirstOrDefault(u => u.UserMasterID == userId);
        }

        /// <summary>
        /// Log wrong password attempt - No cache involved
        /// </summary>
        public async Task LogWrongPassword(int userId, string wrongPassword, string ipAddress, string mobileNumber)
        {
            var log = new WrongPasswordLog
            {
                UserID = userId,
                WrongPassword = wrongPassword, // Production mein hash karo!
                IPAddress = ipAddress,
                MobileNumber = mobileNumber,
                LogAction = "LOGIN_FAILED",
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            await _context.WrongPasswordLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
