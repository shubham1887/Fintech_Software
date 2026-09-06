using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserActionMaster;
using FintechApp.Responses.UserActionMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UserActionMasterService : IUserActionMasterService
    {
        private readonly IUserActionMasterRepository _repository;
        public UserActionMasterService(IUserActionMasterRepository repository) { _repository = repository; }
        private static UserActionMasterResponse MapToResponse(UserActionMaster e) => new() { UserActionMasterID = e.UserActionMasterID, UserID = e.UserID, Type = e.Type, Status = e.Status, Description = e.Description, Platform = e.Platform, FingurePrint = e.FingurePrint, IPAddress = e.IPAddress, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<UserActionMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserActionMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserActionMasterResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUserActionMasterRequest request) { var entity = new UserActionMaster { UserID = request.UserID, Type = request.Type, Status = request.Status, Description = request.Description, Platform = request.Platform, FingurePrint = request.FingurePrint, IPAddress = request.IPAddress, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUserActionMasterRequest request) { var entity = await _repository.GetByID(request.UserActionMasterID); if (entity == null) return Messages.UserActionMasterMessages.NotFound; entity.UserID = request.UserID; entity.Type = request.Type; entity.Status = request.Status; entity.Description = request.Description; entity.Platform = request.Platform; entity.FingurePrint = request.FingurePrint; entity.IPAddress = request.IPAddress; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UserActionMasterMessages.NotFound; }
    }
}
