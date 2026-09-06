using FintechApp.Requests.AEPS;
using FintechApp.Responses.AEPS;

namespace FintechApp.Interfaces.Services
{
    public interface IAEPSService
    {
        Task<(string failureMessage, AEPSSettingResponse? data)> SelectActiveAEPS();
        Task<(string failureMessage, List<AEPSSettingResponse> data)> SelectMultiActiveAEPS();
        Task<(string failureMessage, AEPSTransactionStatusResponse? data)> GetTransactionStatus(AEPSTransactionStatusRequest request, int userId);
        PIDOptionResponse SelectPIDOption(SelectPIDOptionRequest request);
    }
}
