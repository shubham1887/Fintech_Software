using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<long> CreateAuditLog(int userId, string action, string module, string oldValue, string newValue, string ipAddress, string additionalInfo = "")
        {
            var auditLog = new AuditLog
            {
                UserID = userId,
                Action = action,
                Module = module,
                OldValue = oldValue ?? string.Empty,
                NewValue = newValue ?? string.Empty,
                IPAddress = ipAddress,
                AdditionalInfo = additionalInfo,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            return auditLog.AuditLogID;
        }

        public async Task<List<AuditLog>> GetAuditLogsByUserId(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.AuditLogs
                .Where(a => a.UserID == userId && !a.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(a => a.DateTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.DateTime <= toDate.Value);

            return await query
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsByModule(string module, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.AuditLogs
                .Where(a => a.Module == module && !a.IsDeleted);

            if (fromDate.HasValue)
                query = query.Where(a => a.DateTime >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.DateTime <= toDate.Value);

            return await query
                .OrderByDescending(a => a.DateTime)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetRecentAuditLogs(int count = 100)
        {
            return await _context.AuditLogs
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.DateTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetAuditLogById(long auditLogId)
        {
            return await _context.AuditLogs
                .FirstOrDefaultAsync(a => a.AuditLogID == auditLogId && !a.IsDeleted);
        }
    }
}
