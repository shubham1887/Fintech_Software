using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderPurchaseRepository
    {
        Task<List<ProviderPurchase>> GetAll();
        Task<ProviderPurchase?> GetByID(int id);
        Task<List<ProviderPurchase>> GetByProviderID(int providerID);
        Task<int> Insert(ProviderPurchase entity);
        Task<int> Update(ProviderPurchase entity);
        Task<int> Delete(int id);
    }
}
