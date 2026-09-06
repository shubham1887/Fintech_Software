using FintechApp.Requests.AEPSSetting;
using FintechApp.Responses.AEPSSetting;

namespace FintechApp.Interfaces.Services
{
    public interface IAEPSSettingService
    {
        Task<List<AEPSSettingResponse>> GetAll(GetAEPSSettingRequest request);
        Task<string> Create(CreateAEPSSettingRequest request);
        Task<string> Update(UpdateAEPSSettingRequest request);
        Task<string> Delete(DeleteAEPSSettingRequest request);
    }
}
