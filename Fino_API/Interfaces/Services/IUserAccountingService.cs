using FintechApp.Requests.UserAccounting;
using FintechApp.Responses.UserAccounting;

namespace FintechApp.Interfaces.Services
{
    public interface IUserAccountingService
    {
        Task<(string failureMessage, UserwiseAccountingWrapper? data)> UserwiseAccounting(UserwiseAccountingRequest request, int currentUserID);
        Task<(string failureMessage, AmountAdjustmentWrapper? data)> SelectForFilterByOldAmountAdjustment(UserAccountingCompRequest request, int currentUserID);
        Task<(string failureMessage, AmountAdjustmentWrapper? data)> SelectForFilterByNewAmountAdjustment(UserAccountingCompRequest request, int currentUserID);
        Task<(string failureMessage, ManualSuccessFailWrapper? data)> ManualSuccessFail(UserAccountingCompRequest request, int currentUserID);
    }
}
