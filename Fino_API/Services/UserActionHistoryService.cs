using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserActionHistory;
using FintechApp.Responses.UserActionHistory;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UserActionHistoryService : IUserActionHistoryService
    {
        private readonly IUserActionHistoryRepository _repository;
        public UserActionHistoryService(IUserActionHistoryRepository repository) { _repository = repository; }
        private static UserActionHistoryResponse MapToResponse(UserActionHistory e) => new() { UserHistoryID = e.UserHistoryID, UserID = e.UserID, Action = e.Action, Description = e.Description, Status = e.Status, IPAddress = e.IPAddress, };
        public async Task<List<UserActionHistoryResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserActionHistoryResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserActionHistoryResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUserActionHistoryRequest request) { var entity = new UserActionHistory { UserID = request.UserID, Action = request.Action, Description = request.Description, Status = request.Status, IPAddress = request.IPAddress, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUserActionHistoryRequest request) { var entity = await _repository.GetByID(request.UserHistoryID); if (entity == null) return Messages.UserActionHistoryMessages.NotFound; entity.UserID = request.UserID; entity.Action = request.Action; entity.Description = request.Description; entity.Status = request.Status; entity.IPAddress = request.IPAddress; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UserActionHistoryMessages.NotFound; }
    }
}
