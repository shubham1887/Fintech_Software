using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IOTTSettingRepository
    {
        Task<List<OTTSetting>> GetAll();
        Task<OTTSetting?> GetByID(int id);

        Task<int> Insert(OTTSetting entity);
        Task<int> Update(OTTSetting entity);
        Task<int> Delete(int id);
    }
}
