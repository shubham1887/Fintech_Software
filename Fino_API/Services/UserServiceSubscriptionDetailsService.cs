using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserServiceSubscriptionDetails;
using FintechApp.Responses.UserServiceSubscriptionDetails;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UserServiceSubscriptionDetailsService : IUserServiceSubscriptionDetailsService
    {
        private readonly IUserServiceSubscriptionDetailsRepository _repository;
        public UserServiceSubscriptionDetailsService(IUserServiceSubscriptionDetailsRepository repository) { _repository = repository; }
        private static UserServiceSubscriptionDetailsResponse MapToResponse(UserServiceSubscriptionDetails e) => new() { UserServiceSubscriptionDetailsID = e.UserServiceSubscriptionDetailsID, UserID = e.UserID, ServiceID = e.ServiceID, ServiceAndUserwiseSubscriptionID = e.ServiceAndUserwiseSubscriptionID, Charge = e.Charge, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<UserServiceSubscriptionDetailsResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserServiceSubscriptionDetailsResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserServiceSubscriptionDetailsResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUserServiceSubscriptionDetailsRequest request) { var entity = new UserServiceSubscriptionDetails { UserID = request.UserID, ServiceID = request.ServiceID, ServiceAndUserwiseSubscriptionID = request.ServiceAndUserwiseSubscriptionID, Charge = request.Charge, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUserServiceSubscriptionDetailsRequest request) { var entity = await _repository.GetByID(request.UserServiceSubscriptionDetailsID); if (entity == null) return Messages.UserServiceSubscriptionDetailsMessages.NotFound; entity.UserID = request.UserID; entity.ServiceID = request.ServiceID; entity.ServiceAndUserwiseSubscriptionID = request.ServiceAndUserwiseSubscriptionID; entity.Charge = request.Charge; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UserServiceSubscriptionDetailsMessages.NotFound; }
    }
}
