using System.Globalization;
using System.Text;
using System.Text.Json;
using Fino.NexmintApi.Interfaces;
using Fino.NexmintApi.Messages;
using Fino.NexmintApi.Responses;
using Microsoft.Extensions.Logging;

namespace Fino.NexmintApi.Repositories
{
    // Transport/data-access for api.nexmint.in. Issues the HTTP call with the client-id and
    // Authorization headers and maps the outcome to a NexmintApiResponse.
    //
    // Unlike a provider that answers HTTP 200 for everything, Nexmint uses status codes and means
    // them: 400/401/403 is a refusal that reached no money, 5xx (or no reply at all) is an unknown
    // whose payout may still exist upstream. Those two must not collapse into one "failed", so the
    // code is kept on the response and the retryable case is flagged.
    public class NexmintApiClient : INexmintApiClient
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<NexmintApiClient>? _logger;

        public NexmintApiClient(IHttpClientFactory httpFactory, ILogger<NexmintApiClient>? logger = null)
        {
            _httpFactory = httpFactory;
            _logger = logger;
        }

        public async Task<NexmintApiResponse> Send(HttpMethod method, string url, string clientID, string authToken, string? body, int timeoutSeconds)
        {
            string responseBody = string.Empty;
            try
            {
                var client = _httpFactory.CreateClient();
                if (timeoutSeconds > 0) client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

                using var message = new HttpRequestMessage(method, url);
                if (body != null)
                    message.Content = new StringContent(body, Encoding.UTF8, "application/json");

                message.Headers.TryAddWithoutValidation("client-id", clientID);
                message.Headers.TryAddWithoutValidation("Authorization", $"Bearer {authToken}");

                using var resp = await client.SendAsync(message);
                responseBody = await resp.Content.ReadAsStringAsync();
                int code = (int)resp.StatusCode;

                // 5xx: the gateway broke mid-request. The payout may or may not have been created,
                // so the caller has to ask again rather than assume either way.
                if (code >= 500)
                {
                    _logger?.LogWarning("Nexmint HTTP {Status} for {Url}: {Body}", code, url, responseBody);
                    var retry = NexmintApiResponse.Unreachable(Nexmint.Message.ServiceUnavailable);
                    retry.HttpStatus = code;
                    retry.Data = responseBody;
                    return retry;
                }

                var result = Parse(responseBody);
                result.HttpStatus = code;

                // A refusal with no message of its own still has to say something a retailer can read.
                if (!result.ApiSuccess && string.IsNullOrWhiteSpace(result.Message))
                    result.Message = Nexmint.Message.InvalidResponse;

                return result;
            }
            catch (TaskCanceledException ex)
            {
                // Timeout. Same reasoning as a 5xx - unknown, not failed.
                _logger?.LogWarning(ex, "Nexmint call timed out for {Url}", url);
                return NexmintApiResponse.Unreachable(Nexmint.Message.ServiceUnavailable);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Nexmint call failed for {Url}. Body: {Body}", url, responseBody);
                return NexmintApiResponse.Unreachable(Nexmint.Message.ServiceUnavailable);
            }
        }

        // Best-effort parse of the { success, message, data{} } envelope into typed fields; the raw
        // JSON is always retained in Data. Missing/renamed fields degrade to empty/zero, never throw.
        private NexmintApiResponse Parse(string json)
        {
            var result = new NexmintApiResponse { Success = true, Data = json };

            if (string.IsNullOrWhiteSpace(json))
            {
                result.Success = false;
                result.Message = Nexmint.Message.InvalidResponse;
                return result;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    result.Success = false;
                    result.Message = Nexmint.Message.InvalidResponse;
                    return result;
                }

                var envelope = Map(root);
                result.ApiSuccess = Bool(envelope, "success");
                result.Message = Str(envelope, "message");

                // Everything worth reading lives under data{}; a refusal has no data at all, which is
                // why the envelope is read first and this is allowed to be absent.
                if (!envelope.TryGetValue("data", out var dataEl) || dataEl.ValueKind != JsonValueKind.Object)
                    return result;

                Read(result, Map(dataEl));
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning(ex, "Nexmint response was not valid JSON: {Body}", json);
                result.Success = false;
                result.Message = Nexmint.Message.InvalidResponse;
            }

            return result;
        }

        // The transaction fields, which are the same whether they arrive under data{} on a reply or
        // at the top level of a webhook push.
        private static void Read(NexmintApiResponse result, Dictionary<string, JsonElement> data)
        {
            result.Status = Str(data, "status").Trim().ToUpperInvariant();
            result.TransactionID = Str(data, "transaction_id");
            result.OrderID = Str(data, "order_id");
            result.Amount = Dec(data, "amount");
            result.PaymentMethod = Str(data, "payment_method");
            result.UtrNumber = Str(data, "utr_number");
            result.RefundStatus = Str(data, "refund_status");
            result.RefundTransactionID = Str(data, "refund_transaction_id");
            result.Balance = Dec(data, "balance");
            result.HoldBalance = Dec(data, "hold_balance");

            // data.message is the per-transaction wording ("Payment processed successfully"); the
            // envelope's is about the call. Prefer the one that describes the money.
            string dataMessage = Str(data, "message");
            if (dataMessage.Length > 0) result.Message = dataMessage;
        }

        // Field names are documented in snake_case, but a case-sensitive lookup is one provider
        // tweak away from reading every reply as empty - which, on a payout, means refunding a
        // transfer that succeeded.
        internal static Dictionary<string, JsonElement> Map(JsonElement element)
        {
            var map = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in element.EnumerateObject()) map[p.Name] = p.Value;
            return map;
        }

        internal static string Str(Dictionary<string, JsonElement> map, string name)
        {
            if (!map.TryGetValue(name, out var el)) return string.Empty;
            return el.ValueKind switch
            {
                JsonValueKind.String => el.GetString() ?? string.Empty,
                JsonValueKind.Number => el.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => string.Empty
            };
        }

        internal static decimal Dec(Dictionary<string, JsonElement> map, string name)
        {
            if (!map.TryGetValue(name, out var el)) return 0m;
            if (el.ValueKind == JsonValueKind.Number && el.TryGetDecimal(out var d)) return d;
            if (el.ValueKind == JsonValueKind.String &&
                decimal.TryParse(el.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ds))
                return ds;
            return 0m;
        }

        internal static bool Bool(Dictionary<string, JsonElement> map, string name)
        {
            if (!map.TryGetValue(name, out var el)) return false;
            return el.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(el.GetString(), out var b) && b,
                _ => false
            };
        }
    }
}
