using Fino.NexmintApi.Configuration;
using Fino.NexmintApi.Requests;
using Fino.NexmintApi.Responses;

namespace Fino.NexmintApi.Interfaces
{
    // Service layer: typed payout operations over api.nexmint.in. Signs the auth token and the
    // encrypted_payload, builds the endpoint URL, delegates the transport to INexmintApiClient and
    // parses the reply into NexmintApiResponse. Credentials resolve from the optional per-call
    // `creds` override when supplied (e.g. the active payout gateway row), otherwise from the
    // static "NexmintApi" configuration section.
    public interface INexmintApiService
    {
        // Initiate a payout (POST /fund-transfer). The reply is ALWAYS "pending" — the outcome
        // arrives on the webhook, or from GetPayoutStatus.
        Task<NexmintApiResponse> SendPayout(PayoutRequest request, NexmintCredentials? creds = null);

        // Status enquiry (POST /fund-transfer/status), by Nexmint's transaction id or our order id.
        Task<NexmintApiResponse> GetPayoutStatus(PayoutStatusRequest request, NexmintCredentials? creds = null);

        // Wallet enquiry (GET /fund-transfer/balance) — spendable balance plus the amount held
        // against payouts still in flight.
        Task<NexmintApiResponse> GetBalance(NexmintCredentials? creds = null);

        // Verify and read a payout webhook. `authorizationHeader` is the raw Authorization header
        // the push arrived with; the JWT inside it is signed with the webhook secret, NOT the
        // client secret.
        NexmintCallbackResult ParseCallback(string? authorizationHeader, string rawBody, NexmintCredentials? creds = null);
    }
}
