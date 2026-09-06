using FintechApp.Requests.WebHookSetting;
using FintechApp.Responses.WebHookSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IWebHookSettingService
    {
        Task<List<WebHookSettingResponse>> GetAll();
        Task<WebHookSettingResponse?> GetByID(int id);
        Task<string> Create(CreateWebHookSettingRequest request);
        Task<string> Update(UpdateWebHookSettingRequest request);
        Task<string> Delete(int id);
    }
}
