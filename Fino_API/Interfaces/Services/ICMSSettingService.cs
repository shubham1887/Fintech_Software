using FintechApp.Requests.CMSSetting;
using FintechApp.Responses.CMSSetting;
namespace FintechApp.Interfaces.Services
{
    public interface ICMSSettingService
    {
        Task<List<CMSSettingResponse>> GetAll();
        Task<CMSSettingResponse?> GetByID(int id);

        Task<string> Create(CreateCMSSettingRequest request);
        Task<string> Update(UpdateCMSSettingRequest request);
        Task<string> Delete(int id);
    }
}
