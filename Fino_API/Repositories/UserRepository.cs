using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.Requests.User;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<UserMaster>> _listCache;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
            _listCache = new MemoryCacheProcessing<List<UserMaster>>();
        }
        public async Task<List<UserMaster>> GetAllUsers()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUsers, () =>
            {
                return _context.UserMasters
                    .Include(u => u.UserType)
                    .Include(u => u.State)
                    .Where(u => !u.IsDeleted)
                    .AsNoTracking()
                    .ToList();
            }) ?? new List<UserMaster>();
        }

        // Same as GetAllUsers but keeps soft-deleted rows (so the admin All-Users list can show
        // them with a Deleted badge). Separately cached from the non-deleted set.
        public async Task<List<UserMaster>> GetAllUsersWithDeleted()
        {
            return _listCache.GetOrCreate(CacheKeys.AllUsersWithDeleted, () =>
            {
                return _context.UserMasters
                    .Include(u => u.UserType)
                    .Include(u => u.State)
                    .AsNoTracking()
                    .ToList();
            }) ?? new List<UserMaster>();
        }

        public async Task<UserMaster?> GetUserById(int userId)
        {
            var allUsers = await GetAllUsers();
            return allUsers.FirstOrDefault(u => u.UserMasterID == userId);
        }

        public async Task<UserMaster?> GetUserByMobile(string mobileNumber)
        {
            var allUsers = await GetAllUsers();
            return allUsers.FirstOrDefault(u => u.MobileNumber == mobileNumber);
        }

        public async Task<UserMaster?> GetUserByEmail(string email)
        {
            var allUsers = await GetAllUsers();
            return allUsers.FirstOrDefault(u => u.EmailID.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<UserMaster>> GetUsersByUserType(int userTypeId)
        {
            var allUsers = await GetAllUsers();
            return allUsers.Where(u => u.UserTypeID == userTypeId).ToList();
        }

        public async Task<List<UserMaster>> GetUsersByParent(int parentId)
        {
            var allUsers = await GetAllUsers();
            return allUsers.Where(u => u.ParentID == parentId).ToList();
        }

        public async Task<List<UserMaster>> GetUsers(GetUserRequest request)
        {
            var allUsers = await GetAllUsers();
            var query = allUsers.AsQueryable();

            if (request.UserID > 0)
                query = query.Where(u => u.UserMasterID == request.UserID);

            if (!string.IsNullOrEmpty(request.MobileNumber))
                query = query.Where(u => u.MobileNumber == request.MobileNumber);

            if (request.UserTypeID > 0)
                query = query.Where(u => u.UserTypeID == request.UserTypeID);

            query = query.Where(u => u.IsActive == request.IsActive);


            var skip = (request.PageNumber - 1) * request.PageSize;
            return query.Skip(skip).Take(request.PageSize).ToList();
        }

        public async Task<bool> CreateUser(UserMaster user)
        {
            user.DateTime = DateTime.UtcNow;
            user.UpdatedateTime = DateTime.UtcNow;
            user.IsDeleted = false;

            await _context.UserMasters.AddAsync(user);
            var result = await _context.SaveChangesAsync();


            if (result > 0)
            {
                ClearUserCache();
            }

            return result > 0;
        }

        public async Task<bool> UpdateUser(UserMaster user)
        {
            user.UpdatedateTime = DateTime.UtcNow;

            _context.UserMasters.Update(user);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                ClearUserCache();
            }

            return result > 0;
        }

        public async Task<bool> DeleteUser(int userId)
        {
            var user = await _context.UserMasters.FindAsync(userId);
            if (user == null)
                return false;

            user.IsDeleted = true;
            user.UpdatedateTime = DateTime.UtcNow;

            _context.UserMasters.Update(user);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                ClearUserCache();
            }

            return result > 0;
        }

        /// <summary>
        /// Undo a soft delete. The user comes back DEACTIVATED regardless of what they were before
        /// the delete, so restoring a record never puts a live account back on the network by
        /// itself — activating them is a separate, deliberate admin action.
        /// </summary>
        public async Task<bool> RestoreUser(int userId)
        {
            var user = await _context.UserMasters.FindAsync(userId);
            if (user == null)
                return false;

            user.IsDeleted = false;
            user.IsActive = false;
            user.UpdatedateTime = DateTime.UtcNow;

            _context.UserMasters.Update(user);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                ClearUserCache();
            }

            return result > 0;
        }

        public void ClearUserCache()
        {
            _listCache.Remove(CacheKeys.AllUsers);
            _listCache.Remove(CacheKeys.AllUsersWithDeleted);
        }

        public async Task<UserMaster?> GetUserByMobileWithPassword(string mobile, string passwordHash)
        {
            return await _context.UserMasters
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.MobileNumber == mobile && u.PasswordHash == passwordHash && !u.IsDeleted);
        }

        public async Task<UserMaster?> GetUserByIdWithPassword(int userId, string passwordHash)
        {
            return await _context.UserMasters
                .Include(u => u.UserType)
                .FirstOrDefaultAsync(u => u.UserMasterID == userId && u.PasswordHash == passwordHash && !u.IsDeleted);
        }
    }
}
