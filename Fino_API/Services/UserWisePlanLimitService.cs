using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserWisePlanLimit;
using FintechApp.Responses.UserWisePlanLimit;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UserWisePlanLimitService : IUserWisePlanLimitService
    {
        private readonly IUserWisePlanLimitRepository _repository;
        public UserWisePlanLimitService(IUserWisePlanLimitRepository repository) { _repository = repository; }
        private static UserWisePlanLimitResponse MapToResponse(UserWisePlanLimit e) => new() { UserWisePlanLimitID = e.UserWisePlanLimitID, UserID = e.UserID, UserTypeID = e.UserTypeID, CircleHitCount = e.CircleHitCount, PlanHitCount = e.PlanHitCount, BillFetchHitCount = e.BillFetchHitCount, IsActive = e.IsActive, IsDelete = e.IsDelete, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<UserWisePlanLimitResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserWisePlanLimitResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserWisePlanLimitResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUserWisePlanLimitRequest request) { var entity = new UserWisePlanLimit { UserID = request.UserID, UserTypeID = request.UserTypeID, CircleHitCount = request.CircleHitCount, PlanHitCount = request.PlanHitCount, BillFetchHitCount = request.BillFetchHitCount, IsActive = request.IsActive, IsDelete = request.IsDelete, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUserWisePlanLimitRequest request) { var entity = await _repository.GetByID(request.UserWisePlanLimitID); if (entity == null) return Messages.UserWisePlanLimitMessages.NotFound; entity.UserID = request.UserID; entity.UserTypeID = request.UserTypeID; entity.CircleHitCount = request.CircleHitCount; entity.PlanHitCount = request.PlanHitCount; entity.BillFetchHitCount = request.BillFetchHitCount; entity.IsActive = request.IsActive; entity.IsDelete = request.IsDelete; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UserWisePlanLimitMessages.NotFound; }
    }
}
