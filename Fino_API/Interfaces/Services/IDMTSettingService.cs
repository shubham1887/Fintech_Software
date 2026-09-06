using FintechApp.Requests.DMTSetting;
using FintechApp.Responses.DMTSetting;

namespace FintechApp.Interfaces.Services
{
    public interface IDMTSettingService
    {
        Task<List<DMTSettingResponse>> GetAll();
        Task<List<DMTSettingDropdownResponse>> GetDropdown();
        Task<DMTSettingResponse?> GetByID(int dmtSettingId);
        Task<string> Create(CreateDMTSettingRequest request);
        Task<string> Update(UpdateDMTSettingRequest request);
        Task<string> Delete(int dmtSettingId);
        Task<(string failureMessage, List<ActiveDMTListResponse> data)> SelectMultiActiveDMT();
        Task<(string failureMessage, List<TransferModeResponse> data)> SelectTransferMode(string activeDmt);
    }
}
