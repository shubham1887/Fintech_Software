using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderDetailsRepository
    {
        Task<List<ProviderDetails>> GetAll();
        Task<List<ProviderDetails>> GetByProviderID(int providerId);
        Task<ProviderDetails?> GetByID(int providerDetailId);
        Task<int> Insert(ProviderDetails entity);
        Task<int> Update(ProviderDetails entity);
        Task<int> Delete(int providerDetailId);
    }
}
