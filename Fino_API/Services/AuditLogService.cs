using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Responses.AuditLog;

namespace FintechApp.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IUserRepository _userRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository, IUserRepository userRepository)
        {
            _auditLogRepository = auditLogRepository;
            _userRepository = userRepository;
        }

        public async Task LogUserAction(int userId, string action, string module, string oldValue, string newValue, string ipAddress, string additionalInfo = "")
        {
            await _auditLogRepository.CreateAuditLog(userId, action, module, oldValue, newValue, ipAddress, additionalInfo);
        }

        public async Task<List<AuditLogResponse>> GetUserAuditHistory(int userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var logs = await _auditLogRepository.GetAuditLogsByUserId(userId, fromDate, toDate);

            // Get user info for each log
            var userIds = logs.Select(l => l.UserID).Distinct().ToList();
            var allUsers = await _userRepository.GetAllUsers();
            var userDict = allUsers.Where(u => userIds.Contains(u.UserMasterID))
                                   .ToDictionary(u => u.UserMasterID, u => u.PersonName);

            return logs.Select(log => new AuditLogResponse
            {
                AuditLogID = log.AuditLogID,
                UserID = log.UserID,
                UserName = userDict.ContainsKey(log.UserID) ? userDict[log.UserID] : "Unknown",
                Action = log.Action,
                Module = log.Module,
                OldValue = log.OldValue,
                NewValue = log.NewValue,
                IPAddress = log.IPAddress,
                AdditionalInfo = log.AdditionalInfo,
                DateTime = log.DateTime
            }).ToList();
        }

        public async Task<List<AuditLogResponse>> GetModuleAuditHistory(string module, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var logs = await _auditLogRepository.GetAuditLogsByModule(module, fromDate, toDate);

            var userIds = logs.Select(l => l.UserID).Distinct().ToList();
            var allUsers = await _userRepository.GetAllUsers();
            var userDict = allUsers.Where(u => userIds.Contains(u.UserMasterID))
                                   .ToDictionary(u => u.UserMasterID, u => u.PersonName);

            return logs.Select(log => new AuditLogResponse
            {
                AuditLogID = log.AuditLogID,
                UserID = log.UserID,
                UserName = userDict.ContainsKey(log.UserID) ? userDict[log.UserID] : "Unknown",
                Action = log.Action,
                Module = log.Module,
                OldValue = log.OldValue,
                NewValue = log.NewValue,
                IPAddress = log.IPAddress,
                AdditionalInfo = log.AdditionalInfo,
                DateTime = log.DateTime
            }).ToList();
        }

        public async Task<List<AuditLogResponse>> GetRecentAudits(int count = 100)
        {
            var logs = await _auditLogRepository.GetRecentAuditLogs(count);

            var userIds = logs.Select(l => l.UserID).Distinct().ToList();
            var allUsers = await _userRepository.GetAllUsers();
            var userDict = allUsers.Where(u => userIds.Contains(u.UserMasterID))
                                   .ToDictionary(u => u.UserMasterID, u => u.PersonName);

            return logs.Select(log => new AuditLogResponse
            {
                AuditLogID = log.AuditLogID,
                UserID = log.UserID,
                UserName = userDict.ContainsKey(log.UserID) ? userDict[log.UserID] : "Unknown",
                Action = log.Action,
                Module = log.Module,
                OldValue = log.OldValue,
                NewValue = log.NewValue,
                IPAddress = log.IPAddress,
                AdditionalInfo = log.AdditionalInfo,
                DateTime = log.DateTime
            }).ToList();
        }
    }
}
