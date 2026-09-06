using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderRequestResponseLog;
using FintechApp.Responses.ProviderRequestResponseLog;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderRequestResponseLogService : IProviderRequestResponseLogService
    {
        private readonly IProviderRequestResponseLogRepository _repository;
        private readonly IUserRepository _userRepo;
        private readonly IProviderMasterRepository _providerRepo;
        private readonly IServiceRepository _serviceRepo;
        public ProviderRequestResponseLogService(
            IProviderRequestResponseLogRepository repository,
            IUserRepository userRepo,
            IProviderMasterRepository providerRepo,
            IServiceRepository serviceRepo)
        {
            _repository = repository;
            _userRepo = userRepo;
            _providerRepo = providerRepo;
            _serviceRepo = serviceRepo;
        }

        // Fill UserName / ProviderName / ServiceName so the log table can show names (and be
        // filtered by provider + service type) instead of raw ids.
        private async Task Enrich(List<ProviderRequestResponseLogResponse> rows)
        {
            if (rows.Count == 0) return;
            var providerNames = (await _providerRepo.GetAll())
                .GroupBy(p => p.ProviderID).ToDictionary(g => g.Key, g => g.First().ProviderName);
            var serviceNames = (await _serviceRepo.GetAllServices())
                .GroupBy(s => s.ServiceID).ToDictionary(g => g.Key, g => g.First().Name);
            var users = await _userRepo.GetAllUsers();
            var userNames = users.GroupBy(u => u.UserMasterID).ToDictionary(
                g => g.Key,
                g => !string.IsNullOrWhiteSpace(g.First().FirmName) ? g.First().FirmName : g.First().PersonName);
            foreach (var r in rows)
            {
                if (providerNames.TryGetValue(r.ProviderID, out var pn)) r.ProviderName = pn;
                if (serviceNames.TryGetValue(r.ServiceID, out var sn)) r.ServiceName = sn;
                if (userNames.TryGetValue(r.UserID, out var un)) r.UserName = un;
            }
        }

        private static ProviderRequestResponseLogResponse MapToResponse(ProviderRequestResponseLog e) => new()
        {
            ProviderRequestResponseLogID = e.ProviderRequestResponseLogID,
            UserID = e.UserID ?? 0,
            ProviderID = e.ProviderID,
            TransactionID = e.TransactionID,
            Request = e.Request,
            Response = e.Response,
            IPAddress = e.IPAddress,
            Through = e.Through,
            ServiceID = e.ServiceID,
            DateTime = e.DateTime,
        };

        public async Task<List<ProviderRequestResponseLogResponse>> GetAll() { var list = await _repository.GetAll(); var rows = list.OrderByDescending(e => e.ProviderRequestResponseLogID).Select(MapToResponse).ToList(); await Enrich(rows); return rows; }
        public async Task<ProviderRequestResponseLogResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<ProviderRequestResponseLogResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        // Status-check / dispatch logs for one transaction (newest first) — shows whether and when
        // the recharge status was re-checked against the provider.
        public async Task<List<ProviderRequestResponseLogResponse>> GetByTransaction(int transactionId)
        {
            var list = await _repository.GetAll();
            return list.Where(e => e.TransactionID == transactionId)
                .OrderByDescending(e => e.DateTime)
                .Select(MapToResponse).ToList();
        }
        public async Task<string> Create(CreateProviderRequestResponseLogRequest request)
        {
            var entity = new ProviderRequestResponseLog
            {
                UserID = request.UserID,
                ProviderID = request.ProviderID,
                TransactionID = request.TransactionID,
                Request = request.Request,
                Response = request.Response,
                IPAddress = request.IPAddress,
                Through = request.Through,
                ServiceID = request.ServiceID,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateProviderRequestResponseLogRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderRequestResponseLogID);
            if (entity == null) return Messages.ProviderRequestResponseLogMessages.NotFound;
            entity.UserID = request.UserID;
            entity.ProviderID = request.ProviderID;
            entity.TransactionID = request.TransactionID;
            entity.Request = request.Request;
            entity.Response = request.Response;
            entity.IPAddress = request.IPAddress;
            entity.Through = request.Through;
            entity.ServiceID = request.ServiceID;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ProviderRequestResponseLogMessages.NotFound;
        }
    }
}
