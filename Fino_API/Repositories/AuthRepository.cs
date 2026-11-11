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
        /// The account a mobile number signs in as.
        ///
        /// One number can now carry two roles (see StaticModels/LinkedProfilePolicy), so this is
        /// no longer "the row with this number" — it is the PRIMARY of the pair, which is the one
        /// holding the credentials. Resolving it by order of insertion would work today and break
        /// the first time rows come back in a different order.
        /// </summary>
        public async Task<UserMaster?> GetUserByMobileNumber(string mobileNumber)
        {
            var allUsers = await GetAllUsers();
            var matches = allUsers.Where(u => u.MobileNumber == mobileNumber).ToList();
            if (matches.Count <= 1) return matches.FirstOrDefault();

            return matches.FirstOrDefault(u => !u.LinkedPrimaryUserID.HasValue || u.LinkedPrimaryUserID.Value <= 0)
                   ?? matches.OrderBy(u => u.UserMasterID).First();
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
