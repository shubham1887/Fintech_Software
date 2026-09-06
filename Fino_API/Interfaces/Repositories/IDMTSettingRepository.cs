using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IDMTSettingRepository
    {
        Task<List<DMTSetting>> GetAll();
        Task<DMTSetting?> GetByID(int dmtSettingId);
        Task<int> Insert(DMTSetting entity);
        Task<int> Update(DMTSetting entity);
        Task<int> Delete(int dmtSettingId);
    }
}
