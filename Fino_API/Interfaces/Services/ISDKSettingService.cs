using FintechApp.Requests.SDKSetting;
using FintechApp.Responses.SDKSetting;
namespace FintechApp.Interfaces.Services
{
    public interface ISDKSettingService
    {
        Task<List<SDKSettingResponse>> GetAll();
        Task<SDKSettingResponse?> GetByID(int id);

        Task<string> Create(CreateSDKSettingRequest request);
        Task<string> Update(UpdateSDKSettingRequest request);
        Task<string> Delete(int id);
    }
}
