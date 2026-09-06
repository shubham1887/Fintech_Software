using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IAuditLogRepository
    {
        Task<long> CreateAuditLog(int userId, string action, string module, string oldValue, string newValue, string ipAddress, string additionalInfo = "");
        Task<List<AuditLog>> GetAuditLogsByUserId(int userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<AuditLog>> GetAuditLogsByModule(string module, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<AuditLog>> GetRecentAuditLogs(int count = 100);
        Task<AuditLog?> GetAuditLogById(long auditLogId);
    }
}
