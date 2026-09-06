using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SDKBBPSUserMapping;
using FintechApp.Responses.SDKBBPSUserMapping;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class SDKBBPSUserMappingService : ISDKBBPSUserMappingService
    {
        private readonly ISDKBBPSUserMappingRepository _repository;
        public SDKBBPSUserMappingService(ISDKBBPSUserMappingRepository repository) { _repository = repository; }
        private static SDKBBPSUserMappingResponse MapToResponse(SDKBBPSUserMapping e) => new() { SDKBBPSUserMappingID = e.SDKBBPSUserMappingID, UserID = e.UserID, ConfirmationCallbackURL = e.ConfirmationCallbackURL, IsActive = e.IsActive, IsDelete = e.IsDelete, };
        public async Task<List<SDKBBPSUserMappingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<SDKBBPSUserMappingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<SDKBBPSUserMappingResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateSDKBBPSUserMappingRequest request) { var entity = new SDKBBPSUserMapping { UserID = request.UserID, ConfirmationCallbackURL = request.ConfirmationCallbackURL, IsActive = request.IsActive, IsDelete = request.IsDelete, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateSDKBBPSUserMappingRequest request) { var entity = await _repository.GetByID(request.SDKBBPSUserMappingID); if (entity == null) return Messages.SDKBBPSUserMappingMessages.NotFound; entity.UserID = request.UserID; entity.ConfirmationCallbackURL = request.ConfirmationCallbackURL; entity.IsActive = request.IsActive; entity.IsDelete = request.IsDelete; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.SDKBBPSUserMappingMessages.NotFound; }
    }
}
