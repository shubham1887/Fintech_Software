using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IReconciliationSettingRepository
    {
        Task<List<ReconciliationSetting>> GetAll();
        Task<List<ReconciliationSetting>> GetByProviderID(int providerId);
        Task<ReconciliationSetting?> GetByID(int id);
        Task<int> Insert(ReconciliationSetting entity);
        Task<int> Update(ReconciliationSetting entity);
        Task<int> Delete(int id);
    }
}
