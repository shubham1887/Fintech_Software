using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.DailyAuthenticationMaster;
using FintechApp.Responses.DailyAuthentication;
using FintechApp.Responses.DailyAuthenticationMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class DailyAuthenticationMasterService : IDailyAuthenticationMasterService
    {
        private readonly IDailyAuthenticationMasterRepository _repository;
        public DailyAuthenticationMasterService(IDailyAuthenticationMasterRepository repository) { _repository = repository; }
        private static DailyAuthenticationMasterResponse MapToResponse(DailyAuthenticationMaster e) => new() { AuthenticationID = e.AuthenticationID, UserID = e.UserID, ProviderID = e.ProviderID, ServiceID = e.ServiceID, OperatorID = e.OperatorID, Pipe = e.Pipe, };
        public async Task<List<DailyAuthenticationMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<DailyAuthenticationMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<DailyAuthenticationMasterResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateDailyAuthenticationMasterRequest request) { var entity = new DailyAuthenticationMaster { UserID = request.UserID, ProviderID = request.ProviderID, ServiceID = request.ServiceID, OperatorID = request.OperatorID, Pipe = request.Pipe, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateDailyAuthenticationMasterRequest request) { var entity = await _repository.GetByID(request.AuthenticationID); if (entity == null) return Messages.DailyAuthenticationMasterMessages.NotFound; entity.UserID = request.UserID; entity.ProviderID = request.ProviderID; entity.ServiceID = request.ServiceID; entity.OperatorID = request.OperatorID; entity.Pipe = request.Pipe; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.DailyAuthenticationMasterMessages.NotFound; }

        public async Task<List<DailyAuthenticationMasterResponse>> GetReport(int userId, int providerId, DateTime? fromDate, DateTime? toDate)
        {
            var all = await _repository.GetByUserID(userId);
            var filtered = all.AsQueryable();
            if (providerId > 0) filtered = filtered.Where(e => e.ProviderID == providerId);
            if (fromDate.HasValue) filtered = filtered.Where(e => e.DateTime >= fromDate.Value);
            if (toDate.HasValue) filtered = filtered.Where(e => e.DateTime <= toDate.Value);
            return filtered.OrderByDescending(e => e.DateTime).Select(MapToResponse).ToList();
        }

        public async Task<DailyAuthLast2FAResponse> GetLast2FA(int userId, int providerId, bool isMerchantVerify)
        {
            var all = await _repository.GetByUserID(userId);
            var latest = all.Where(e => e.ProviderID == providerId).OrderByDescending(e => e.DateTime).FirstOrDefault();

            var response = new DailyAuthLast2FAResponse
            {
                IsMerchantVerify = isMerchantVerify,
                Is2FA = true
            };

            if (latest == null) return response;

            response.AuthenticationID = latest.AuthenticationID;
            response.UserID = latest.UserID;
            response.ProviderID = latest.ProviderID;
            response.ServiceID = latest.ServiceID;
            response.OperatorID = latest.OperatorID;
            response.Pipe = latest.Pipe;
            response.DOCDateTime = latest.DateTime;
            response.Is2FA = latest.DateTime.Date < DateTime.Today;
            return response;
        }
    }
}
