using FintechApp.Requests.ServiceSubscriptionPlanDetails;
using FintechApp.Responses.ServiceSubscriptionPlanDetails;
namespace FintechApp.Interfaces.Services
{
    public interface IServiceSubscriptionPlanDetailsService
    {
        Task<List<ServiceSubscriptionPlanDetailsResponse>> GetAll();
        Task<ServiceSubscriptionPlanDetailsResponse?> GetByID(int id);

        Task<string> Create(CreateServiceSubscriptionPlanDetailsRequest request);
        Task<string> Update(UpdateServiceSubscriptionPlanDetailsRequest request);
        Task<string> Delete(int id);
    }
}
