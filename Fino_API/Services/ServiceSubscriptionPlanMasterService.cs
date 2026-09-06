using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ServiceSubscriptionPlanMaster;
using FintechApp.Responses.ServiceSubscriptionPlanMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ServiceSubscriptionPlanMasterService : IServiceSubscriptionPlanMasterService
    {
        private readonly IServiceSubscriptionPlanMasterRepository _repository;
        public ServiceSubscriptionPlanMasterService(IServiceSubscriptionPlanMasterRepository repository) { _repository = repository; }

        private static ServiceSubscriptionPlanMasterResponse MapToResponse(ServiceSubscriptionPlanMaster e) => new()
        {
            ServiceSubscriptionPlanMasterID = e.ServiceSubscriptionPlanMasterID,
            PlanName = e.PlanName,
        };

        public async Task<List<ServiceSubscriptionPlanMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ServiceSubscriptionPlanMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateServiceSubscriptionPlanMasterRequest request)
        {
            var entity = new ServiceSubscriptionPlanMaster
            {
                PlanName = request.PlanName,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateServiceSubscriptionPlanMasterRequest request)
        {
            var entity = await _repository.GetByID(request.ServiceSubscriptionPlanMasterID);
            if (entity == null) return Messages.ServiceSubscriptionPlanMasterMessages.NotFound;
            entity.PlanName = request.PlanName;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ServiceSubscriptionPlanMasterMessages.NotFound;
        }
    }
}
