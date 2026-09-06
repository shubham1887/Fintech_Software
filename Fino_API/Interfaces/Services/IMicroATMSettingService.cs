using FintechApp.Requests.MicroATMSetting;
using FintechApp.Responses.MicroATMSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IMicroATMSettingService
    {
        Task<List<MicroATMSettingResponse>> GetAll();
        Task<MicroATMSettingResponse?> GetByID(int id);

        Task<string> Create(CreateMicroATMSettingRequest request);
        Task<string> Update(UpdateMicroATMSettingRequest request);
        Task<string> Delete(int id);
    }
}
