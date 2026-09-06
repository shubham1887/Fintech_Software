using Fino.JaiJuiApi.Configuration;
using Fino.JaiJuiApi.Requests;
using Fino.JaiJuiApi.Responses;

namespace Fino.JaiJuiApi.Interfaces
{
    // Service layer: typed DMT operations over jaijuirecharge.in. Builds the credential header and
    // JSON body, delegates the transport to IJaiJuiApiClient, and parses the provider response into
    // JaiJuiApiResponse. Credentials resolve from the optional per-call `creds` override when supplied
    // (e.g. the active DMT gateway row), otherwise from the static "JaiJuiApi" configuration section.
    public interface IJaiJuiApiService
    {
        // Account/penny verify (IsVerify=true) — validate a beneficiary account before transfer.
        Task<JaiJuiApiResponse> AccountVerify(AccountVerifyRequest request, JaiJuiCredentials? creds = null);

        // Money transfer (IsVerify=false) — dispatch a DMT transfer to a bank account.
        Task<JaiJuiApiResponse> MoneyTransfer(MoneyTransferRequest request, JaiJuiCredentials? creds = null);

        // Status enquiry — re-check the outcome of a previously submitted transfer.
        Task<JaiJuiApiResponse> StatusCheck(StatusCheckRequest request, JaiJuiCredentials? creds = null);

        // Balance enquiry — fetch the provider wallet balance for the given credentials.
        Task<JaiJuiApiResponse> GetBalance(JaiJuiCredentials? creds = null);
    }
}
