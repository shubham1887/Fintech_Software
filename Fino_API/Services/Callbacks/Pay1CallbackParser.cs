using FintechApp.Entities;
using FintechApp.Interfaces.Services;
using FintechApp.Providers;
using FintechApp.Requests.TransactionProcessing;
using FintechApp.StaticModels;
using Fino.Pay1Api.Interfaces;

namespace FintechApp.Services.Callbacks
{
    /// <summary>
    /// Pay1's auto status-update push. All it does is hand the values to Fino.Pay1Api and map the
    /// verified result onto the host's neutral shape — the signature rule
    /// (<c>sha1(partner_id + client_req_id + api_key)</c>) and the field names stay inside the
    /// provider's own library, which is where a Pay1 change should be a one-project edit.
    /// </summary>
    public class Pay1CallbackParser : IProviderCallbackParser
    {
        private readonly IPay1ApiService _pay1;

        public Pay1CallbackParser(IPay1ApiService pay1) => _pay1 = pay1;

        public bool CanHandle(ProviderMaster provider) => Pay1ProviderResolver.IsPay1(provider.ProviderName);

        public ProviderCallbackParseResult Parse(ProviderMaster provider, IDictionary<string, string> values, string rawBody,
            IDictionary<string, string> headers)
        {
            var push = _pay1.ParseCallback(values, rawBody, Pay1ProviderResolver.CredentialsFrom(provider));
            if (!push.Verified) return ProviderCallbackParseResult.Reject(push.Message);

            string description = push.Description;
            if (push.ErrorCode.Length > 0 && push.ErrorCode != "0")
                description = description.Length > 0 ? $"{description} ({push.ErrorCode})" : push.ErrorCode;

            return ProviderCallbackParseResult.Accept(new RechargeCallbackRequest
            {
                RequestID = push.RequestID,
                // Prefer the biller's own reference; fall back to Pay1's transaction id.
                ReferenceNo = push.OperatorID.Length > 0 ? push.OperatorID : push.TransactionID,
                Status = push.Status,
                Balance = push.ClosingBalance,
                Description = description,
                IsReversal = push.IsReversal,
            }, push.Message);
        }
    }
}
