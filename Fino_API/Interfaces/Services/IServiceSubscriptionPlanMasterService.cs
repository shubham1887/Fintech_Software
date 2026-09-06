using FintechApp.Requests.ServiceSubscriptionPlanMaster;
using FintechApp.Responses.ServiceSubscriptionPlanMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IServiceSubscriptionPlanMasterService
    {
        Task<List<ServiceSubscriptionPlanMasterResponse>> GetAll();
        Task<ServiceSubscriptionPlanMasterResponse?> GetByID(int id);
        Task<string> Create(CreateServiceSubscriptionPlanMasterRequest request);
        Task<string> Update(UpdateServiceSubscriptionPlanMasterRequest request);
        Task<string> Delete(int id);
    }
}
