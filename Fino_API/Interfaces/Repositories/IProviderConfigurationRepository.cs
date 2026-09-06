using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderConfigurationRepository
    {
        Task<List<ProviderConfiguration>> GetAll();
        Task<List<ProviderConfiguration>> GetByProviderID(int providerId);
        Task<List<ProviderConfiguration>> GetByOperatorID(int operatorId);
        Task<ProviderConfiguration?> GetByID(int configurationId);
        Task<int> Insert(ProviderConfiguration entity);
        Task<int> Update(ProviderConfiguration entity);
        Task<int> Delete(int configurationId);
    }
}
