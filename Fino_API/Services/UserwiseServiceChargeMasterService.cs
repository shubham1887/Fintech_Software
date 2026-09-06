using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserwiseServiceChargeMaster;
using FintechApp.Responses.UserwiseServiceChargeMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UserwiseServiceChargeMasterService : IUserwiseServiceChargeMasterService
    {
        private readonly IUserwiseServiceChargeMasterRepository _repository;
        public UserwiseServiceChargeMasterService(IUserwiseServiceChargeMasterRepository repository) { _repository = repository; }
        private static UserwiseServiceChargeMasterResponse MapToResponse(UserwiseServiceChargeMaster e) => new() { UserwiseServiceChargeMasterID = e.UserwiseServiceChargeMasterID, ServiceID = e.ServiceID, UserID = e.UserID, OneTime = e.OneTime, IsDelete = e.IsDelete, };
        public async Task<List<UserwiseServiceChargeMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserwiseServiceChargeMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserwiseServiceChargeMasterResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUserwiseServiceChargeMasterRequest request) { var entity = new UserwiseServiceChargeMaster { ServiceID = request.ServiceID, UserID = request.UserID, OneTime = request.OneTime, IsDelete = request.IsDelete, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUserwiseServiceChargeMasterRequest request) { var entity = await _repository.GetByID(request.UserwiseServiceChargeMasterID); if (entity == null) return Messages.UserwiseServiceChargeMasterMessages.NotFound; entity.ServiceID = request.ServiceID; entity.UserID = request.UserID; entity.OneTime = request.OneTime; entity.IsDelete = request.IsDelete; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UserwiseServiceChargeMasterMessages.NotFound; }
    }
}
