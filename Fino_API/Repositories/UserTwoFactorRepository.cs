using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    /// <summary>
    /// Storage for authenticator enrolments.
    ///
    /// Note the absence of the MemoryCacheProcessing list cache every other repository here uses.
    /// That cache is right for reference data and wrong for a credential: it is keyed per
    /// repository instance and invalidated by hand, so a disabled enrolment could keep verifying
    /// until something else happened to clear it. Reads go to the database every time — there is
    /// at most one per sign-in.
    /// </summary>
    public class UserTwoFactorRepository : IUserTwoFactorRepository
    {
        private readonly ApplicationDbContext _context;

        public UserTwoFactorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserTwoFactor?> GetByUserID(int userId) =>
            await _context.UserTwoFactors.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserID == userId && !x.IsDeleted);

        public async Task<UserTwoFactor> StartEnrolment(int userId, string secretBase32)
        {
            // Hard-delete any previous row. The unique index on UserID does not know about
            // IsDeleted, and more importantly a replaced secret has no reason to survive: keeping
            // it would leave a second set of working codes in the database.
            var existing = await _context.UserTwoFactors.Where(x => x.UserID == userId).ToListAsync();
            if (existing.Count > 0) _context.UserTwoFactors.RemoveRange(existing);

            var row = new UserTwoFactor
            {
                UserID = userId,
                SecretBase32 = secretBase32,
                IsConfirmed = false,
                LastUsedStep = 0,
                RecoveryCodeHashes = string.Empty,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            };
            await _context.UserTwoFactors.AddAsync(row);
            await _context.SaveChangesAsync();
            return row;
        }

        public async Task<int> Update(UserTwoFactor entity)
        {
            entity.UpdatedateTime = DateTime.UtcNow;
            _context.UserTwoFactors.Update(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteForUser(int userId)
        {
            var rows = await _context.UserTwoFactors.Where(x => x.UserID == userId).ToListAsync();
            if (rows.Count == 0) return 0;
            _context.UserTwoFactors.RemoveRange(rows);
            return await _context.SaveChangesAsync();
        }
    }
}
