using FintechApp.Requests.UISetting;
using FintechApp.Responses.UISetting;

namespace FintechApp.Interfaces.Services
{
    public interface IUISettingService
    {
        Task<UISettingResponse?> GetUISettingById(int uiSettingMasterID);
        Task<List<UISettingResponse>> GetUISettings(GetUISettingRequest request);
        Task<string> CreateUISetting(CreateUISettingRequest request);
        Task<string> UpdateUISetting(UpdateUISettingRequest request);
        Task<string> DeleteUISetting(int uiSettingMasterID);
    }
}