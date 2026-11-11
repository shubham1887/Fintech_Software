using System.Globalization;
using Fino.NexmintApi.Interfaces;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Providers;
using FintechApp.Requests.TransactionProcessing;
using FintechApp.StaticModels;

namespace FintechApp.Services.Callbacks
{
    /// <summary>
    /// Nexmint's payout webhook — the only way a payout reaches its final state without someone
    /// asking. Initiation always answers "pending"; this is where success or failure arrives.
    ///
    /// All it does is hand the push to Fino.NexmintApi and map the verified result onto the host's
    /// neutral shape. The signature rule (a JWT in <c>Authorization</c>, signed with the webhook
    /// secret — a DIFFERENT key from the client secret the API calls use) and the field names stay
    /// inside the provider's own library, so a Nexmint change is a one-project edit.
    ///
    /// The push carries no wallet balance, so none is reported: writing a zero would overwrite a
    /// real balance on the transaction with a number the provider never sent.
    /// </summary>
    public class NexmintCallbackParser : IProviderCallbackParser
    {
        private readonly INexmintApiService _nexmint;

        public NexmintCallbackParser(INexmintApiService nexmint) => _nexmint = nexmint;

        public bool CanHandle(ProviderMaster provider) => NexmintProviderResolver.IsNexmint(provider.ProviderName);

        public ProviderCallbackParseResult Parse(ProviderMaster provider, IDictionary<string, string> values, string rawBody,
            IDictionary<string, string> headers)
        {
            string authorization = CallbackValues.First(headers, "Authorization");
            var push = _nexmint.ParseCallback(authorization, rawBody, NexmintProviderResolver.CredentialsFrom(provider));
            if (!push.Verified) return ProviderCallbackParseResult.Reject(push.Message);

            // The order id is ours — it is the ProviderRequestID the transfer was sent with, which is
            // what the settlement path looks the transaction up by. Without it there is nothing to
            // settle, and guessing from the amount would settle the wrong transfer.
            if (string.IsNullOrWhiteSpace(push.OrderID))
                return ProviderCallbackParseResult.Reject(Messages.Provider.Message.CallbackNoReference);

            string description = push.Description;
            // On a failure Nexmint also refunds OUR wallet with them, in its own time. That is not the
            // retailer's refund — the pipeline still has to make that one — but the reconciler needs
            // to see which side has happened.
            if (push.RefundStatus.Length > 0)
            {
                string refund = push.RefundTransactionID.Length > 0
                    ? $"refund {push.RefundStatus} ({push.RefundTransactionID})"
                    : $"refund {push.RefundStatus}";
                description = description.Length > 0 ? $"{description}. {refund}" : refund;
            }

            return ProviderCallbackParseResult.Accept(new RechargeCallbackRequest
            {
                RequestID = push.OrderID,
                // The bank's UTR is the proof of payment; their transaction id is the fallback while
                // a UTR does not exist (a failure never has one).
                ReferenceNo = push.UtrNumber.Length > 0 ? push.UtrNumber : push.TransactionID,
                Status = push.Status,
                Amount = push.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                Description = description,
            }, push.Message);
        }
    }
}
