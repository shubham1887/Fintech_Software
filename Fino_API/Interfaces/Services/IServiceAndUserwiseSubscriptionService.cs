using FintechApp.Requests.ServiceAndUserwiseSubscription;
using FintechApp.Responses.ServiceAndUserwiseSubscription;
namespace FintechApp.Interfaces.Services
{
    public interface IServiceAndUserwiseSubscriptionService
    {
        Task<List<ServiceAndUserwiseSubscriptionResponse>> GetAll();
        Task<ServiceAndUserwiseSubscriptionResponse?> GetByID(int id);
        Task<List<ServiceAndUserwiseSubscriptionResponse>> GetByUserID(int userID);
        Task<string> Create(CreateServiceAndUserwiseSubscriptionRequest request);
        Task<string> Update(UpdateServiceAndUserwiseSubscriptionRequest request);
        Task<string> Delete(int id);
    }
}
