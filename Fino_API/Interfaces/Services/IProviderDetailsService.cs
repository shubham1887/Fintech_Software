using FintechApp.Requests.ProviderDetails;
using FintechApp.Responses.ProviderDetails;

namespace FintechApp.Interfaces.Services
{
    public interface IProviderDetailsService
    {
        Task<List<ProviderDetailsResponse>> GetAll();
        Task<List<ProviderDetailsResponse>> GetByProviderID(int providerId);
        Task<ProviderDetailsResponse?> GetByID(int providerDetailId);
        Task<string> Create(CreateProviderDetailsRequest request);
        Task<string> Update(UpdateProviderDetailsRequest request);
        Task<string> Delete(int providerDetailId);
    }
}
