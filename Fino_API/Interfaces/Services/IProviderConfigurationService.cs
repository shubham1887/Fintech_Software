using FintechApp.Requests.ProviderConfiguration;
using FintechApp.Responses.ProviderConfiguration;

namespace FintechApp.Interfaces.Services
{
    public interface IProviderConfigurationService
    {
        Task<List<ProviderConfigurationResponse>> GetAll();
        Task<List<ProviderConfigurationResponse>> GetByProviderID(int providerId);
        Task<List<ProviderConfigurationResponse>> GetByOperatorID(int operatorId);
        Task<ProviderConfigurationResponse?> GetByID(int configurationId);
        Task<string> Create(CreateProviderConfigurationRequest request);
        Task<string> Update(UpdateProviderConfigurationRequest request);
        Task<string> Delete(int configurationId);
    }
}
