using FintechApp.Requests.Beneficiary;
using FintechApp.Responses.Beneficiary;

namespace FintechApp.Interfaces.Services
{
    // Beneficiary account verification: penny-drops the account through the active DMT gateway
    // (JaiJui AccountVerify) to fetch the bank's account-holder name, charging a configurable fee
    // from the retailer's DMT wallet (ledgered, and refunded automatically if verification fails).
    public interface IDMTAccountVerifyService
    {
        Task<(string failureMessage, VerifyAccountResponse? response)> VerifyAccount(
            VerifyAccountRequest request, int userId, string ipAddress);
    }
}
