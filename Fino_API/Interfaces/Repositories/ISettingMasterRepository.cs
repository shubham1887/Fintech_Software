using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ISettingMasterRepository
    {
        Task<List<SettingMaster>> GetAll();
        Task<SettingMaster?> GetByID(int settingMasterId);
        Task<SettingMaster?> GetByKey(string key);
        Task<int> Insert(SettingMaster entity);
        Task<int> Update(SettingMaster entity);
        Task<int> Delete(int settingMasterId);
    }
}
