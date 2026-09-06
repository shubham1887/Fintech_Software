using FintechApp.Requests.ProviderStateConfiguration;
using FintechApp.Responses.ProviderStateConfiguration;
namespace FintechApp.Interfaces.Services
{
    public interface IProviderStateConfigurationService
    {
        Task<List<ProviderStateConfigurationResponse>> GetAll();
        Task<List<ProviderStateConfigurationResponse>> GetByProviderID(int providerId);
        Task<ProviderStateConfigurationResponse?> GetByID(int id);
        Task<string> Create(CreateProviderStateConfigurationRequest request);
        Task<string> Update(UpdateProviderStateConfigurationRequest request);
        Task<string> Delete(int id);
    }
}
