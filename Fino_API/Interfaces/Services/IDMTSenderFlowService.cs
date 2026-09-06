using FintechApp.Requests.DMTSender;
using FintechApp.Responses.DMTSender;

namespace FintechApp.Interfaces.Services
{
    public interface IDMTSenderFlowService
    {
        Task<(string result, string failureMessage)> AddSender(AddSenderRequest request, int userId, string ipAddress);
        Task<string> VerifyOTP(VerifySenderOTPRequest request, int userId);
        Task<string> ResendOTP(ResendSenderOTPRequest request, int userId);
        Task<(string failureMessage, SenderDetailsResponse? response)> GetSender(SelectSenderRequest request, int userId);
        Task<(string failureMessage, List<SenderByAccountResponse> response)> GetByAccountNumber(SelectSenderByAccountRequest request);
    }
}
