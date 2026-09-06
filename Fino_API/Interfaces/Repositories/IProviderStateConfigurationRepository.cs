using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderStateConfigurationRepository
    {
        Task<List<ProviderStateConfiguration>> GetAll();
        Task<List<ProviderStateConfiguration>> GetByProviderID(int providerId);
        Task<ProviderStateConfiguration?> GetByID(int id);
        Task<int> Insert(ProviderStateConfiguration entity);
        Task<int> Update(ProviderStateConfiguration entity);
        Task<int> Delete(int id);
    }
}
