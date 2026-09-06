using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SupportUserAccessMaster;
using FintechApp.Responses.SupportUserAccessMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class SupportUserAccessMasterService : ISupportUserAccessMasterService
    {
        private readonly ISupportUserAccessMasterRepository _repository;
        public SupportUserAccessMasterService(ISupportUserAccessMasterRepository repository) { _repository = repository; }
        private static SupportUserAccessMasterResponse MapToResponse(SupportUserAccessMaster e) => new() { SupportUserAccessMasterID = e.SupportUserAccessMasterID, MenuMasterID = e.MenuMasterID, UserID = e.UserID, MenuGUID = e.MenuGUID, };
        public async Task<List<SupportUserAccessMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<SupportUserAccessMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<SupportUserAccessMasterResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateSupportUserAccessMasterRequest request) { var entity = new SupportUserAccessMaster { MenuMasterID = request.MenuMasterID, UserID = request.UserID, MenuGUID = request.MenuGUID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateSupportUserAccessMasterRequest request) { var entity = await _repository.GetByID(request.SupportUserAccessMasterID); if (entity == null) return Messages.SupportUserAccessMasterMessages.NotFound; entity.MenuMasterID = request.MenuMasterID; entity.UserID = request.UserID; entity.MenuGUID = request.MenuGUID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.SupportUserAccessMasterMessages.NotFound; }
    }
}
