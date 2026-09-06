using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.AuthorizationMaster;
using FintechApp.Responses.AuthorizationMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class AuthorizationMasterService : IAuthorizationMasterService
    {
        private readonly IAuthorizationMasterRepository _repository;
        public AuthorizationMasterService(IAuthorizationMasterRepository repository) { _repository = repository; }
        private static AuthorizationMasterResponse MapToResponse(AuthorizationMaster e) => new() { AuthorizationMasterID = e.AuthorizationMasterID, Controller = e.Controller, Action = e.Action, UserTypeID = e.UserTypeID, IsActive = e.IsActive, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<AuthorizationMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<AuthorizationMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateAuthorizationMasterRequest request) { var entity = new AuthorizationMaster { Controller = request.Controller, Action = request.Action, UserTypeID = request.UserTypeID, IsActive = request.IsActive, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateAuthorizationMasterRequest request) { var entity = await _repository.GetByID(request.AuthorizationMasterID); if (entity == null) return Messages.AuthorizationMasterMessages.NotFound; entity.Controller = request.Controller; entity.Action = request.Action; entity.UserTypeID = request.UserTypeID; entity.IsActive = request.IsActive; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.AuthorizationMasterMessages.NotFound; }
    }
}
