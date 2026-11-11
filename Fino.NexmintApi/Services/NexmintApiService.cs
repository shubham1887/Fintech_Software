using System.Text;
using System.Text.Json;
using Fino.NexmintApi.Configuration;
using Fino.NexmintApi.Interfaces;
using Fino.NexmintApi.Messages;
using Fino.NexmintApi.Repositories;
using Fino.NexmintApi.Requests;
using Fino.NexmintApi.Responses;
using Microsoft.Extensions.Configuration;

namespace Fino.NexmintApi.Services
{
    // Builds api.nexmint.in endpoint URLs, signs the two HS256 tokens every call carries, and
    // delegates the HTTP exchange to INexmintApiClient. Endpoint paths and payload field names match
    // the provider specification exactly. Credentials come from the optional per-call override (the
    // active payout gateway row) when supplied, otherwise from the "NexmintApi" configuration
    // section - fully dynamic either way.
    public class NexmintApiService : INexmintApiService
    {
        private const string DefaultBaseUrl = "https://api.nexmint.in/api/v1/gateway";
        private const string SandboxSegment = "sandbox";
        private const string PayoutPath = "fund-transfer";
        private const string StatusPath = "fund-transfer/status";
        private const string BalancePath = "fund-transfer/balance";
        private const string BearerPrefix = "Bearer ";
        private const int RemarkMaxLength = 250;
        private const int MobileLength = 10;

        private readonly INexmintApiClient _client;
        private readonly IConfiguration _config;

        public NexmintApiService(INexmintApiClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
        }

        // Resolve effective options: start from the "NexmintApi" config section, then overlay any
        // non-blank fields from the per-call credentials override.
        private NexmintApiOptions Options(NexmintCredentials? creds)
        {
            var o = new NexmintApiOptions();
            _config.GetSection(NexmintApiOptions.SectionName).Bind(o);

            if (creds != null)
            {
                if (!string.IsNullOrWhiteSpace(creds.BaseUrl)) o.BaseUrl = creds.BaseUrl;
                if (!string.IsNullOrWhiteSpace(creds.ClientID)) o.ClientID = creds.ClientID;
                if (!string.IsNullOrWhiteSpace(creds.ClientSecret)) o.ClientSecret = creds.ClientSecret;
                if (!string.IsNullOrWhiteSpace(creds.WebhookSecret)) o.WebhookSecret = creds.WebhookSecret;
                if (creds.UseSandbox.HasValue) o.UseSandbox = creds.UseSandbox.Value;
            }

            if (string.IsNullOrWhiteSpace(o.BaseUrl)) o.BaseUrl = DefaultBaseUrl;
            o.BaseUrl = o.BaseUrl.TrimEnd('/');

            // An operator who pastes the sandbox URL into the provider row has already said what they
            // want; appending /sandbox again would 404 and read as "the provider is down".
            if (o.BaseUrl.EndsWith("/" + SandboxSegment, StringComparison.OrdinalIgnoreCase))
            {
                o.BaseUrl = o.BaseUrl[..^(SandboxSegment.Length + 1)];
                o.UseSandbox = true;
            }

            return o;
        }

        private static string Url(NexmintApiOptions o, string path) =>
            o.UseSandbox ? $"{o.BaseUrl}/{SandboxSegment}/{path}" : $"{o.BaseUrl}/{path}";

        public Task<NexmintApiResponse> SendPayout(PayoutRequest request, NexmintCredentials? creds = null)
        {
            var o = Options(creds);
            if (NotConfigured(o)) return Refuse(Nexmint.Message.NotConfigured);

            string name = Letters(request.AccountHolderName);
            string bank = Letters(request.BankName);
            string mobile = Mobile(request.Mobile);
            // Nexmint truncates decimals itself; doing it here means the amount we record as sent is
            // the amount that was sent, instead of a rupee the retailer was charged and nobody paid.
            long amount = (long)decimal.Truncate(request.Amount);

            if (string.IsNullOrWhiteSpace(request.OrderID)) return Refuse(Nexmint.Message.OrderIDRequired);
            if (string.IsNullOrWhiteSpace(request.AccountNumber)) return Refuse(Nexmint.Message.AccountNumberRequired);
            if (string.IsNullOrWhiteSpace(request.IFSC)) return Refuse(Nexmint.Message.IFSCRequired);
            if (name.Length == 0) return Refuse(Nexmint.Message.BeneficiaryNameRequired);
            if (bank.Length == 0) return Refuse(Nexmint.Message.BankNameRequired);
            if (mobile.Length != MobileLength) return Refuse(Nexmint.Message.MobileRequired);
            if (amount < 1) return Refuse(Nexmint.Message.AmountRequired);

            var payload = new Dictionary<string, object?>
            {
                ["order_id"] = request.OrderID.Trim(),
                ["payment_method"] = PaymentMethod(request.PaymentMethod),
                ["account_holder_name"] = name,
                ["bank_name"] = bank,
                ["account_number"] = request.AccountNumber.Trim(),
                ["ifsc_code"] = request.IFSC.Trim().ToUpperInvariant(),
                ["mobile"] = mobile,
                ["amount"] = amount,
                ["account_type"] = AccountType(request.AccountType),
                ["remark"] = Remark(request.Remark)
            };

            return Post(o, PayoutPath, payload);
        }

        public Task<NexmintApiResponse> GetPayoutStatus(PayoutStatusRequest request, NexmintCredentials? creds = null)
        {
            var o = Options(creds);
            if (NotConfigured(o)) return Refuse(Nexmint.Message.NotConfigured);

            bool hasTransaction = !string.IsNullOrWhiteSpace(request.TransactionID);
            bool hasOrder = !string.IsNullOrWhiteSpace(request.OrderID);
            if (!hasTransaction && !hasOrder) return Refuse(Nexmint.Message.ReferenceRequired);

            // Sending both is allowed and transaction_id wins upstream, but a stale pair is a quiet
            // way to ask about the wrong transfer. Send the one that identifies it exactly.
            var payload = new Dictionary<string, object?>();
            if (hasTransaction) payload["transaction_id"] = request.TransactionID.Trim();
            else payload["order_id"] = request.OrderID.Trim();

            return Post(o, StatusPath, payload);
        }

        public Task<NexmintApiResponse> GetBalance(NexmintCredentials? creds = null)
        {
            var o = Options(creds);
            if (NotConfigured(o)) return Refuse(Nexmint.Message.NotConfigured);

            // GET: the two headers and nothing else - no body, no encrypted_payload.
            return _client.Send(HttpMethod.Get, Url(o, BalancePath), o.ClientID,
                Security.NexmintJwt.AuthToken(o.ClientID, o.ClientSecret), null, o.TimeoutSeconds);
        }

        public NexmintCallbackResult ParseCallback(string? authorizationHeader, string rawBody, NexmintCredentials? creds = null)
        {
            var o = Options(creds);

            string token = (authorizationHeader ?? string.Empty).Trim();
            if (token.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
                token = token[BearerPrefix.Length..].Trim();

            // A callback settles or refunds a transaction. Without a secret configured we cannot tell
            // Nexmint from anyone who found the URL, so there is nothing safe to do but refuse.
            if (string.IsNullOrWhiteSpace(o.WebhookSecret) || token.Length == 0)
                return NexmintCallbackResult.Reject(Nexmint.Message.CallbackUnsigned);

            if (!Security.NexmintJwt.TryVerify(token, o.WebhookSecret, out _))
                return NexmintCallbackResult.Reject(Nexmint.Message.CallbackSignatureInvalid);

            try
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(rawBody) ? "{}" : rawBody);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                    return NexmintCallbackResult.Reject(Nexmint.Message.InvalidResponse);

                var body = NexmintApiClient.Map(doc.RootElement);
                return new NexmintCallbackResult
                {
                    Verified = true,
                    Message = Nexmint.Message.CallbackVerified,
                    OrderID = NexmintApiClient.Str(body, "order_id"),
                    TransactionID = NexmintApiClient.Str(body, "transaction_id"),
                    Status = NexmintApiClient.Str(body, "status").Trim().ToUpperInvariant(),
                    UtrNumber = NexmintApiClient.Str(body, "utr_number"),
                    Amount = NexmintApiClient.Dec(body, "amount"),
                    RefundStatus = NexmintApiClient.Str(body, "refund_status"),
                    RefundTransactionID = NexmintApiClient.Str(body, "refund_transaction_id"),
                    Description = NexmintApiClient.Str(body, "message")
                };
            }
            catch (JsonException)
            {
                return NexmintCallbackResult.Reject(Nexmint.Message.InvalidResponse);
            }
        }

        // Both tokens are minted here, immediately before the call, because Nexmint rejects anything
        // whose iat is more than five seconds old.
        private Task<NexmintApiResponse> Post(NexmintApiOptions o, string path, Dictionary<string, object?> payload)
        {
            string authToken = Security.NexmintJwt.AuthToken(o.ClientID, o.ClientSecret);
            string encrypted = Security.NexmintJwt.Sign(payload, o.ClientSecret);
            string body = JsonSerializer.Serialize(new { encrypted_payload = encrypted });

            return _client.Send(HttpMethod.Post, Url(o, path), o.ClientID, authToken, body, o.TimeoutSeconds);
        }

        private static bool NotConfigured(NexmintApiOptions o) =>
            string.IsNullOrWhiteSpace(o.ClientID) || string.IsNullOrWhiteSpace(o.ClientSecret);

        // A refusal, not an outage: nothing was sent, so the caller may fail the transfer and refund
        // rather than leave it pending.
        private static Task<NexmintApiResponse> Refuse(string message) =>
            Task.FromResult(NexmintApiResponse.Fail(message));

        // Nexmint strips everything but letters and spaces from the name and the bank before it
        // validates them, then stores what is left. Doing the same here keeps our record of the
        // beneficiary identical to theirs, and turns "rejected after sending" into a local check.
        private static string Letters(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var sb = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                if (char.IsLetter(c)) sb.Append(c);
                else if (char.IsWhiteSpace(c) || c == '.' || c == '-') sb.Append(' ');
            }

            return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        // Exactly ten digits. A stored "+91 98765 43210" is the same number, and refusing it here
        // would be our formatting rule, not the bank's.
        private static string Mobile(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var digits = new StringBuilder(value.Length);
            foreach (char c in value) if (char.IsDigit(c)) digits.Append(c);

            string number = digits.ToString();
            return number.Length > MobileLength ? number[^MobileLength..] : number;
        }

        // Payout runs on the bank rails only. A transfer asked for over UPI has no equivalent here,
        // so it goes IMPS - instant, same as UPI would have been - rather than being refused.
        private static string PaymentMethod(string? value)
        {
            string method = (value ?? string.Empty).Trim().ToUpperInvariant();
            return method switch
            {
                Nexmint.PaymentMethod.NEFT => Nexmint.PaymentMethod.NEFT,
                Nexmint.PaymentMethod.RTGS => Nexmint.PaymentMethod.RTGS,
                _ => Nexmint.PaymentMethod.IMPS
            };
        }

        private static string AccountType(string? value) =>
            (value ?? string.Empty).Trim().StartsWith("cur", StringComparison.OrdinalIgnoreCase)
                ? Nexmint.AccountType.Current
                : Nexmint.AccountType.Saving;

        // Required upstream, 1-250 characters. An empty remark would fail validation after the call,
        // so it is filled here with something a reconciler can actually use.
        private static string Remark(string? value)
        {
            string remark = (value ?? string.Empty).Trim();
            if (remark.Length == 0) remark = DefaultRemark;
            return remark.Length > RemarkMaxLength ? remark[..RemarkMaxLength] : remark;
        }

        private const string DefaultRemark = "Payout";
    }
}
