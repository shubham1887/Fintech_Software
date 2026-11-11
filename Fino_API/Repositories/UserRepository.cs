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

        /// <summary>
        /// Save an edited user.
        ///
        /// Callers get their UserMaster from the cached, AsNoTracking list, which is loaded with
        /// its UserType and State navigations attached. Handing that whole graph to
        /// <c>DbSet.Update</c> made EF treat the navigation as the authority on the foreign key
        /// and quietly reset UserTypeID back to the UserTypeMaster still hanging off the entity —
        /// which is why changing a user's role on the Edit User form saved without complaint and
        /// changed nothing. It also marked the principal rows themselves as modified, so an
        /// ordinary profile edit rewrote a shared master row.
        ///
        /// Loading the tracked row and copying the SCALARS across fixes both: CurrentValues
        /// carries the columns (foreign keys included) and touches no navigation, and the
        /// caller's own object is left intact for whatever it does next.
        /// </summary>
        public async Task<bool> UpdateUser(UserMaster user)
        {
            user.UpdatedateTime = DateTime.UtcNow;

            var tracked = await _context.UserMasters.FirstOrDefaultAsync(u => u.UserMasterID == user.UserMasterID);
            if (tracked == null) return false;

            var entry = _context.Entry(tracked);
            // The value as the DATABASE has it, not as the in-memory object has it. Some callers
            // hand back an entity this context is already tracking and have mutated it in place,
            // so reading the property would just return the new hash and the comparison below
            // would never fire.
            string previousHash = entry.Property(u => u.PasswordHash).OriginalValue ?? string.Empty;
            entry.CurrentValues.SetValues(user);

            // One mobile number can carry two roles, and they are one person with one password
            // (StaticModels/LinkedProfilePolicy). Mirroring the hash here rather than at each
            // call site means every way a password can change — self-service, forgot-password,
            // admin reset, expiry — keeps the pair in step, and none of them has to remember to.
            if ((tracked.PasswordHash ?? string.Empty) != previousHash)
                await MirrorPasswordToLinkedLogins(tracked);

            await _context.SaveChangesAsync();

            // Callers mutate the cached instance in place before calling this, so the cache is
            // already dirty whether or not the write landed. Dropping it either way keeps a
            // failed save from leaving an edit that never happened visible to every request.
            ClearUserCache();

            // The row exists and the save did not throw. A save that moved no columns (the same
            // values re-submitted) returns 0 rows affected and is still a success — reporting it
            // as "User update failed" is what a row-count check would do here.
            return true;
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

        /// <summary>
        /// Switch the two-step-verification flag, touching nothing else on the row.
        /// Clears the user cache so the very next sign-in reads the new value.
        /// </summary>
        public async Task<bool> SetTwoFactorEnabled(int userId, bool enabled)
        {
            var user = await _context.UserMasters.FindAsync(userId);
            if (user == null)
                return false;

            if (user.IsTwoFA == enabled)
                return true; // already in the wanted state — nothing to write

            user.IsTwoFA = enabled;
            user.UpdatedateTime = DateTime.UtcNow;

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                ClearUserCache();
            }

            return result > 0;
        }

        /// <summary>
        /// End every live session this user has, by moving their session generation on by one.
        ///
        /// Access tokens are signed JWTs carrying the generation they were minted under, so this
        /// one write invalidates all of them at once - the tab that asked to sign out, the other
        /// tab that did not, and any copy of the token taken off the wire. Nothing else on the row
        /// is touched, and the user cache is cleared so the very next request reads the new value
        /// rather than a cached row that still says the old generation is current.
        /// </summary>
        public async Task<bool> BumpSessionEpoch(int userId)
        {
            var user = await _context.UserMasters.FindAsync(userId);
            if (user == null)
                return false;

            // Unchecked: this only ever needs to DIFFER from what live tokens carry, so wrapping
            // round after two billion sign-outs is harmless and never throws.
            unchecked { user.SessionEpoch = user.SessionEpoch + 1; }
            user.UpdatedateTime = DateTime.UtcNow;

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                ClearUserCache();
            }

            return result > 0;
        }

        /// <summary>
        /// Copy a changed password hash onto the other login(s) of the same person. Tracked
        /// entities, so they are written by the caller's own SaveChanges.
        /// </summary>
        private async Task MirrorPasswordToLinkedLogins(UserMaster changed)
        {
            int primaryID = changed.LinkedPrimaryUserID is int linked && linked > 0
                ? linked
                : changed.UserMasterID;

            var siblings = await _context.UserMasters
                .Where(u => u.UserMasterID != changed.UserMasterID
                            && (u.UserMasterID == primaryID || u.LinkedPrimaryUserID == primaryID))
                .ToListAsync();

            foreach (var sibling in siblings)
            {
                sibling.PasswordHash = changed.PasswordHash;
                sibling.UpdatedateTime = DateTime.UtcNow;
            }
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
