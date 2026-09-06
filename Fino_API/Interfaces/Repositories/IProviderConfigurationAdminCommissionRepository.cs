using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderConfigurationAdminCommissionRepository
    {
        Task<List<ProviderConfigurationAdminCommission>> GetAll();
        Task<ProviderConfigurationAdminCommission?> GetByID(int id);
        Task<List<ProviderConfigurationAdminCommission>> GetByProviderID(int providerID);
        Task<int> Insert(ProviderConfigurationAdminCommission entity);
        Task<int> Update(ProviderConfigurationAdminCommission entity);
        Task<int> Delete(int id);
    }
}
