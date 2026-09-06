using FintechApp.Requests.HitachiYesBankAEPS;
using FintechApp.Responses.HitachiYesBankAEPS;

namespace FintechApp.Interfaces.Services
{
    public interface IHitachiYesBankAEPSService
    {
        Task<(string failureMessage, YesBank_AuthenticationResponse data)> Authentication(YesBank_AuthenticationRequest request, int userID);
        Task<(string failureMessage, AEPSViewModelResponse data)> CashWithdrawal(YesBank_CashWithdrawalRequest request, int userID, string ipAddress, string userAgent);
        Task<(string failureMessage, AEPSViewModelResponse data)> BalanceEnquiry(YesBank_BalanceEnquiryRequest request, int userID, string ipAddress, string userAgent);
        Task<(string failureMessage, AEPSViewModelResponse data)> MiniStatement(YesBank_MinistatementRequest request, int userID, string ipAddress, string userAgent);
    }
}
