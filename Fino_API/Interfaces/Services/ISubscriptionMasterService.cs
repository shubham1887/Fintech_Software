using FintechApp.Requests.SubscriptionMaster;
using FintechApp.Responses.SubscriptionMaster;
namespace FintechApp.Interfaces.Services
{
    public interface ISubscriptionMasterService
    {
        Task<List<SubscriptionMasterResponse>> GetAll();
        Task<SubscriptionMasterResponse?> GetByID(int id);

        Task<string> Create(CreateSubscriptionMasterRequest request);
        Task<string> Update(UpdateSubscriptionMasterRequest request);
        Task<string> Delete(int id);
    }
}
