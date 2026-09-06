using FintechApp.Requests.eKYC;
using FintechApp.Responses.eKYC;

namespace FintechApp.Interfaces.Services
{
    public interface IeKYCService
    {
        Task<(string failureMessage, VerifyPanResponse? data)> VerifyPan(VerifyPanRequest request);
        Task<(string failureMessage, Yes_eKYCInitiateResponse data)> YesBankEkycInitiate(int userID);
        Task<(string failureMessage, Yes_eKYCValidateResponse data)> YesBankEkycValidate(eKYCValidateRequest request, int userID);
        Task<(string failureMessage, Yes_eKYCValidateResponse data)> YesBankBioMatricVerification(YesBank_AadharBiometricRequest request, int userID);
    }
}
