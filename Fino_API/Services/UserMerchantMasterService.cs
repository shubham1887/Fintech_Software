using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserMerchantMaster;
using FintechApp.Responses.UserMerchantMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UserMerchantMasterService : IUserMerchantMasterService
    {
        private readonly IUserMerchantMasterRepository _repository;
        public UserMerchantMasterService(IUserMerchantMasterRepository repository) { _repository = repository; }
        private static UserMerchantMasterResponse MapToResponse(UserMerchantMaster e) => new() { UserMerchantMasterID = e.UserMerchantMasterID, UserID = e.UserID, PaySprintUPIMerchantCode = e.PaySprintUPIMerchantCode, PaySprintStaticQR = e.PaySprintStaticQR, PaySprintUPIRefid = e.PaySprintUPIRefid, };
        public async Task<List<UserMerchantMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserMerchantMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserMerchantMasterResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUserMerchantMasterRequest request) { var entity = new UserMerchantMaster { UserID = request.UserID, PaySprintUPIMerchantCode = request.PaySprintUPIMerchantCode, PaySprintStaticQR = request.PaySprintStaticQR, PaySprintUPIRefid = request.PaySprintUPIRefid, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUserMerchantMasterRequest request) { var entity = await _repository.GetByID(request.UserMerchantMasterID); if (entity == null) return Messages.UserMerchantMasterMessages.NotFound; entity.UserID = request.UserID; entity.PaySprintUPIMerchantCode = request.PaySprintUPIMerchantCode; entity.PaySprintStaticQR = request.PaySprintStaticQR; entity.PaySprintUPIRefid = request.PaySprintUPIRefid; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UserMerchantMasterMessages.NotFound; }
    }
}
