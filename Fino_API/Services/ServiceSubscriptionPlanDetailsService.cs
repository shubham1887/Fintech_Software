using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ServiceSubscriptionPlanDetails;
using FintechApp.Responses.ServiceSubscriptionPlanDetails;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class ServiceSubscriptionPlanDetailsService : IServiceSubscriptionPlanDetailsService
    {
        private readonly IServiceSubscriptionPlanDetailsRepository _repository;
        public ServiceSubscriptionPlanDetailsService(IServiceSubscriptionPlanDetailsRepository repository) { _repository = repository; }
        private static ServiceSubscriptionPlanDetailsResponse MapToResponse(ServiceSubscriptionPlanDetails e) => new() { ServiceSubscriptionPlanDetailsID = e.ServiceSubscriptionPlanDetailsID, ServiceSubscriptionCreatePlanMasterID = e.ServiceSubscriptionCreatePlanMasterID, ServiceID = e.ServiceID, DateTime = e.DateTime, UpdatedateTime = e.UpdatedateTime };
        public async Task<List<ServiceSubscriptionPlanDetailsResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ServiceSubscriptionPlanDetailsResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateServiceSubscriptionPlanDetailsRequest request) { var entity = new ServiceSubscriptionPlanDetails { ServiceSubscriptionCreatePlanMasterID = request.ServiceSubscriptionCreatePlanMasterID, ServiceID = request.ServiceID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateServiceSubscriptionPlanDetailsRequest request) { var entity = await _repository.GetByID(request.ServiceSubscriptionPlanDetailsID); if (entity == null) return Messages.ServiceSubscriptionPlanDetailsMessages.NotFound; entity.ServiceSubscriptionCreatePlanMasterID = request.ServiceSubscriptionCreatePlanMasterID; entity.ServiceID = request.ServiceID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.ServiceSubscriptionPlanDetailsMessages.NotFound; }
    }
}
