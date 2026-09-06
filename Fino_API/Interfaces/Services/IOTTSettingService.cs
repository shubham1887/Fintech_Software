using FintechApp.Requests.OTTSetting;
using FintechApp.Responses.OTTSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IOTTSettingService
    {
        Task<List<OTTSettingResponse>> GetAll();
        Task<OTTSettingResponse?> GetByID(int id);

        Task<string> Create(CreateOTTSettingRequest request);
        Task<string> Update(UpdateOTTSettingRequest request);
        Task<string> Delete(int id);
    }
}
