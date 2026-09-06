using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserWisePlanView;
using FintechApp.Responses.UserWisePlanView;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UserWisePlanViewService : IUserWisePlanViewService
    {
        private readonly IUserWisePlanViewRepository _repository;
        public UserWisePlanViewService(IUserWisePlanViewRepository repository) { _repository = repository; }
        private static UserWisePlanViewResponse MapToResponse(UserWisePlanView e) => new() { UserWisePlanViewID = e.UserWisePlanViewID, UserID = e.UserID, ServiceID = e.ServiceID, OperatorID = e.OperatorID, ConsumerNumber = e.ConsumerNumber, ProviderID = e.ProviderID, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<UserWisePlanViewResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserWisePlanViewResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserWisePlanViewResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUserWisePlanViewRequest request) { var entity = new UserWisePlanView { UserID = request.UserID, ServiceID = request.ServiceID, OperatorID = request.OperatorID, ConsumerNumber = request.ConsumerNumber, ProviderID = request.ProviderID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUserWisePlanViewRequest request) { var entity = await _repository.GetByID(request.UserWisePlanViewID); if (entity == null) return Messages.UserWisePlanViewMessages.NotFound; entity.UserID = request.UserID; entity.ServiceID = request.ServiceID; entity.OperatorID = request.OperatorID; entity.ConsumerNumber = request.ConsumerNumber; entity.ProviderID = request.ProviderID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UserWisePlanViewMessages.NotFound; }
    }
}
