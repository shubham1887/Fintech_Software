using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserServiceSubscriptionCharge;
using FintechApp.Responses.UserServiceSubscriptionCharge;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UserServiceSubscriptionChargeService : IUserServiceSubscriptionChargeService
    {
        private readonly IUserServiceSubscriptionChargeRepository _repository;
        public UserServiceSubscriptionChargeService(IUserServiceSubscriptionChargeRepository repository) { _repository = repository; }
        private static UserServiceSubscriptionChargeResponse MapToResponse(UserServiceSubscriptionCharge e) => new() { UserServiceSubscriptionChargeID = e.UserServiceSubscriptionChargeID, UserID = e.UserID, ServiceID = e.ServiceID, ServiceAndUserwiseSubscriptionID = e.ServiceAndUserwiseSubscriptionID, Charge = e.Charge, Sequence = e.Sequence, IsAdminApproved = e.IsAdminApproved, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<UserServiceSubscriptionChargeResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserServiceSubscriptionChargeResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserServiceSubscriptionChargeResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUserServiceSubscriptionChargeRequest request) { var entity = new UserServiceSubscriptionCharge { UserID = request.UserID, ServiceID = request.ServiceID, ServiceAndUserwiseSubscriptionID = request.ServiceAndUserwiseSubscriptionID, Charge = request.Charge, Sequence = request.Sequence, IsAdminApproved = request.IsAdminApproved, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUserServiceSubscriptionChargeRequest request) { var entity = await _repository.GetByID(request.UserServiceSubscriptionChargeID); if (entity == null) return Messages.UserServiceSubscriptionChargeMessages.NotFound; entity.UserID = request.UserID; entity.ServiceID = request.ServiceID; entity.ServiceAndUserwiseSubscriptionID = request.ServiceAndUserwiseSubscriptionID; entity.Charge = request.Charge; entity.Sequence = request.Sequence; entity.IsAdminApproved = request.IsAdminApproved; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UserServiceSubscriptionChargeMessages.NotFound; }
    }
}
