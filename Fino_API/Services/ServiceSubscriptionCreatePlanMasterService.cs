using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ServiceSubscriptionCreatePlanMaster;
using FintechApp.Responses.ServiceSubscriptionCreatePlanMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class ServiceSubscriptionCreatePlanMasterService : IServiceSubscriptionCreatePlanMasterService
    {
        private readonly IServiceSubscriptionCreatePlanMasterRepository _repository;
        public ServiceSubscriptionCreatePlanMasterService(IServiceSubscriptionCreatePlanMasterRepository repository) { _repository = repository; }
        private static ServiceSubscriptionCreatePlanMasterResponse MapToResponse(ServiceSubscriptionCreatePlanMaster e) => new() { ServiceSubscriptionCreatePlanMasterID = e.ServiceSubscriptionCreatePlanMasterID, PlanName = e.PlanName, Amount = e.Amount, ImageURL = e.ImageURL, IsDefault = e.IsDefault, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<ServiceSubscriptionCreatePlanMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ServiceSubscriptionCreatePlanMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateServiceSubscriptionCreatePlanMasterRequest request) { var entity = new ServiceSubscriptionCreatePlanMaster { PlanName = request.PlanName, Amount = request.Amount, ImageURL = request.ImageURL, IsDefault = request.IsDefault, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateServiceSubscriptionCreatePlanMasterRequest request) { var entity = await _repository.GetByID(request.ServiceSubscriptionCreatePlanMasterID); if (entity == null) return Messages.ServiceSubscriptionCreatePlanMasterMessages.NotFound; entity.PlanName = request.PlanName; entity.Amount = request.Amount; entity.ImageURL = request.ImageURL; entity.IsDefault = request.IsDefault; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.ServiceSubscriptionCreatePlanMasterMessages.NotFound; }
    }
}
