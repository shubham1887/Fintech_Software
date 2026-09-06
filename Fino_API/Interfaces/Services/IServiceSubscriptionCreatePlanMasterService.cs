using FintechApp.Requests.ServiceSubscriptionCreatePlanMaster;
using FintechApp.Responses.ServiceSubscriptionCreatePlanMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IServiceSubscriptionCreatePlanMasterService
    {
        Task<List<ServiceSubscriptionCreatePlanMasterResponse>> GetAll();
        Task<ServiceSubscriptionCreatePlanMasterResponse?> GetByID(int id);

        Task<string> Create(CreateServiceSubscriptionCreatePlanMasterRequest request);
        Task<string> Update(UpdateServiceSubscriptionCreatePlanMasterRequest request);
        Task<string> Delete(int id);
    }
}
