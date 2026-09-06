using FintechApp.Requests;
using FintechApp.Responses;
using FintechApp.Responses.AuditLog;

namespace FintechApp.Interfaces.Services
{
    public interface IAuditLogService
    {
        Task LogUserAction(int userId, string action, string module, string oldValue, string newValue, string ipAddress, string additionalInfo = "");
        Task<List<AuditLogResponse>> GetUserAuditHistory(int userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<AuditLogResponse>> GetModuleAuditHistory(string module, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<AuditLogResponse>> GetRecentAudits(int count = 100);
    }
}
