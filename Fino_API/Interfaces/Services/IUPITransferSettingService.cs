using FintechApp.Requests.UPITransferSetting;
using FintechApp.Responses.UPITransferSetting;
namespace FintechApp.Interfaces.Services
{
    public interface IUPITransferSettingService
    {
        Task<List<UPITransferSettingResponse>> GetAll();
        Task<UPITransferSettingResponse?> GetByID(int id);

        Task<string> Create(CreateUPITransferSettingRequest request);
        Task<string> Update(UpdateUPITransferSettingRequest request);
        Task<string> Delete(int id);
    }
}
