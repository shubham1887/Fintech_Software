using FintechApp.Requests.PinGeneration;
using FintechApp.Responses.PinGeneration;

namespace FintechApp.Interfaces.Services
{
    public interface IPinGenerationService
    {
        Task<string> GeneratePinOTP(PinGenerationRequest request);
        Task<string> ResendPinOTP(PinGenerationRequest request);
        Task<(string failureMessage, VerifyPinOTPResponse? response)> VerifyPinOTP(PinVerifyRequest request);
        Task<string> ForgotMpinGenerateOTP(ForgotMpinRequest request);
    }
}
