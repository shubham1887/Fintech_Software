using FintechApp.Requests.SettingMaster;
using FintechApp.Responses.SettingMaster;

namespace FintechApp.Interfaces.Services
{
    public interface ISettingMasterService
    {
        Task<List<SettingMasterResponse>> GetAll();
        Task<SettingMasterResponse?> GetByID(int settingMasterId);
        Task<string> Create(CreateSettingMasterRequest request);
        Task<string> Update(UpdateSettingMasterRequest request);
        Task<string> Delete(int settingMasterId);
    }
}
