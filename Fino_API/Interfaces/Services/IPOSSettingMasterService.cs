using FintechApp.Requests.POSSettingMaster;
using FintechApp.Responses.POSSettingMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IPOSSettingMasterService
    {
        Task<List<POSSettingMasterResponse>> GetAll();
        Task<POSSettingMasterResponse?> GetByID(int id);

        Task<string> Create(CreatePOSSettingMasterRequest request);
        Task<string> Update(UpdatePOSSettingMasterRequest request);
        Task<string> Delete(int id);
    }
}
