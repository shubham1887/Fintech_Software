using FintechApp.Requests.Transaction;
using FintechApp.Responses.Transaction;

namespace FintechApp.Interfaces.Services
{
    // Full recharge transaction pipeline: validate → balance check → create PENDING transaction →
    // debit wallet → dispatch to operator (IRechargeProviderClient) → settle (success/refund).
    public interface IRechargeService
    {
        Task<(string failureMessage, RechargeResponse? data)> Recharge(RechargeRequest request, int userID, string ipAddress);
    }
}
