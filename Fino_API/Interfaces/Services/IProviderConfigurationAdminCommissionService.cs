using FintechApp.Requests.ProviderConfigurationAdminCommission;
using FintechApp.Responses.ProviderConfigurationAdminCommission;
namespace FintechApp.Interfaces.Services
{
    public interface IProviderConfigurationAdminCommissionService
    {
        Task<List<ProviderConfigurationAdminCommissionResponse>> GetAll();
        Task<ProviderConfigurationAdminCommissionResponse?> GetByID(int id);
        Task<List<ProviderConfigurationAdminCommissionResponse>> GetByProviderID(int providerID);
        Task<string> Create(CreateProviderConfigurationAdminCommissionRequest request);
        Task<string> Update(UpdateProviderConfigurationAdminCommissionRequest request);
        Task<string> Delete(int id);
    }
}
