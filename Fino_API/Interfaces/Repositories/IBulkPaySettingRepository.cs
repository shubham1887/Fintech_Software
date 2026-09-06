using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IBulkPaySettingRepository
    {
        Task<List<BulkPaySetting>> GetAll();
        Task<BulkPaySetting?> GetByID(int id);

        Task<int> Insert(BulkPaySetting entity);
        Task<int> Update(BulkPaySetting entity);
        Task<int> Delete(int id);
    }
}
