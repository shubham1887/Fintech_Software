using FintechApp.Requests.ProviderPurchase;
using FintechApp.Responses.ProviderPurchase;
namespace FintechApp.Interfaces.Services
{
    public interface IProviderPurchaseService
    {
        Task<List<ProviderPurchaseResponse>> GetAll();
        Task<ProviderPurchaseResponse?> GetByID(int id);
        Task<List<ProviderPurchaseResponse>> GetByProviderID(int providerID);
        Task<string> Create(CreateProviderPurchaseRequest request);
        Task<string> Update(UpdateProviderPurchaseRequest request);
        Task<string> Delete(int id);
    }
}
