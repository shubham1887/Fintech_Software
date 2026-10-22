using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Phase4;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IPeriodLockService" />
    public class PeriodLockService : IPeriodLockService
    {
        private readonly ApplicationDbContext _context;
        public PeriodLockService(ApplicationDbContext context) => _context = context;

        public async Task<bool> IsLocked(string periodKey) =>
            await _context.AcctPeriodLocks.AsNoTracking()
                .AnyAsync(l => l.PeriodKey == periodKey && !l.IsDeleted);

        public async Task<string> Lock(PeriodLockRequest request)
        {
            string key = request.PeriodKey.Trim();
            if (key.Length != 6) return "A period (yyyyMM) is required.";
            if (await IsLocked(key)) return StatusType.Ok; // already locked, idempotent

            _context.AcctPeriodLocks.Add(new AcctPeriodLock
            {
                PeriodKey = key,
                LockedOn = DateTime.UtcNow,
                LockedByUserID = request.CreatedByUserID,
                Note = request.Note.Trim(),
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        public async Task<string> Unlock(PeriodLockRequest request)
        {
            string key = request.PeriodKey.Trim();
            var locks = await _context.AcctPeriodLocks.Where(l => l.PeriodKey == key && !l.IsDeleted).ToListAsync();
            if (locks.Count == 0) return StatusType.Ok;
            foreach (var l in locks) { l.IsDeleted = true; l.UpdatedateTime = DateTime.UtcNow; }
            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        public async Task<List<AcctPeriodLock>> GetLocks() =>
            await _context.AcctPeriodLocks.AsNoTracking()
                .Where(l => !l.IsDeleted).OrderByDescending(l => l.PeriodKey).ToListAsync();
    }
}
