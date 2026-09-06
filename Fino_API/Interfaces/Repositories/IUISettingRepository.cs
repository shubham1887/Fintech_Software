using FintechApp.Entities;
using FintechApp.Requests.UISetting;

namespace FintechApp.Interfaces.Repositories
{
    public interface IUISettingRepository
    {
        Task<List<UISettingMaster>> GetAllUISettings();
        Task<UISettingMaster?> GetUISettingById(int uiSettingMasterID);
        Task<UISettingMaster?> GetUISettingByKey(string key);
        Task<List<UISettingMaster>> GetUISettings(GetUISettingRequest request);
        Task<int> Insert(UISettingMaster uiSetting);
        Task<int> Update(UISettingMaster uiSetting);
        Task<int> Delete(UISettingMaster uiSetting);
    }
}