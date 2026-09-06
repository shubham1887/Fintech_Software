using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UserPlanUtilization;
using FintechApp.Responses.UserPlanUtilization;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UserPlanUtilizationService : IUserPlanUtilizationService
    {
        private readonly IUserPlanUtilizationRepository _repository;
        public UserPlanUtilizationService(IUserPlanUtilizationRepository repository) { _repository = repository; }
        private static UserPlanUtilizationResponse MapToResponse(UserPlanUtilization e) => new() { UserPlanUtilizationID = e.UserPlanUtilizationID, UserID = e.UserID, CircleFetchCount = e.CircleFetchCount, PlanFetchCount = e.PlanFetchCount, BillFetchCount = e.BillFetchCount, TotalPlanCount = e.TotalPlanCount, TotalCircleCount = e.TotalCircleCount, TotalBillFetchCount = e.TotalBillFetchCount, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<UserPlanUtilizationResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UserPlanUtilizationResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<UserPlanUtilizationResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateUserPlanUtilizationRequest request) { var entity = new UserPlanUtilization { UserID = request.UserID, CircleFetchCount = request.CircleFetchCount, PlanFetchCount = request.PlanFetchCount, BillFetchCount = request.BillFetchCount, TotalPlanCount = request.TotalPlanCount, TotalCircleCount = request.TotalCircleCount, TotalBillFetchCount = request.TotalBillFetchCount, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUserPlanUtilizationRequest request) { var entity = await _repository.GetByID(request.UserPlanUtilizationID); if (entity == null) return Messages.UserPlanUtilizationMessages.NotFound; entity.UserID = request.UserID; entity.CircleFetchCount = request.CircleFetchCount; entity.PlanFetchCount = request.PlanFetchCount; entity.BillFetchCount = request.BillFetchCount; entity.TotalPlanCount = request.TotalPlanCount; entity.TotalCircleCount = request.TotalCircleCount; entity.TotalBillFetchCount = request.TotalBillFetchCount; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UserPlanUtilizationMessages.NotFound; }
    }
}
