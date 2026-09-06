using System.Text;
using System.Text.Json;
using Fino.JaiJuiApi.Interfaces;
using Fino.JaiJuiApi.Messages;
using Fino.JaiJuiApi.Responses;
using Microsoft.Extensions.Logging;

namespace Fino.JaiJuiApi.Repositories
{
    // Transport/data-access for jaijuirecharge.in. Issues the HTTP POST (JSON body + "KeyData"
    // credential header) and maps the outcome to a JaiJuiApiResponse. The provider returns HTTP 200
    // even for a declined transfer, so the body is parsed regardless of status code; only a transport
    // failure (network error / non-200 / unparseable body) collapses to Success=false.
    public class JaiJuiApiClient : IJaiJuiApiClient
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<JaiJuiApiClient>? _logger;

        public JaiJuiApiClient(IHttpClientFactory httpFactory, ILogger<JaiJuiApiClient>? logger = null)
        {
            _httpFactory = httpFactory;
            _logger = logger;
        }

        public async Task<JaiJuiApiResponse> Post(string url, string keyData, string body)
        {
            string responseBody = string.Empty;
            try
            {
                var client = _httpFactory.CreateClient();

                using var message = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
                // Per spec the credentials travel in a JSON-string header named "KeyData".
                message.Headers.TryAddWithoutValidation("KeyData", keyData);

                using var resp = await client.SendAsync(message);
                responseBody = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    _logger?.LogWarning("JaiJui HTTP {Status} for {Url}: {Body}", (int)resp.StatusCode, url, responseBody);
                    return JaiJuiApiResponse.Fail(JaiJui.Message.ServiceUnavailable);
                }

                return Parse(responseBody);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "JaiJui call failed for {Url}. Body: {Body}", url, responseBody);
                return JaiJuiApiResponse.Fail(JaiJui.Message.ServiceUnavailable);
            }
        }

        // Best-effort parse of the JaiJui response envelope into typed fields; the raw JSON is always
        // retained in Data. Missing/renamed fields degrade gracefully to empty/zero rather than throwing.
        private JaiJuiApiResponse Parse(string json)
        {
            var result = new JaiJuiApiResponse { Success = true, Data = json };

            if (string.IsNullOrWhiteSpace(json)) return result;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return result;

                // JaiJui returns camelCase field names ("status", "message", "transactionID", …), but
                // JsonElement.TryGetProperty is CASE-SENSITIVE. Reading "Status"/"Message" directly made
                // every reply parse as an empty status → the transfer router treated it as FAILURE and
                // refunded even genuine successes. Build a case-insensitive lookup so casing never matters.
                var map = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in root.EnumerateObject()) map[p.Name] = p.Value;

                result.Status = Str(map, "Status");
                result.Message = Str(map, "Message");
                result.APIUserRequestID = Str(map, "APIUserRequestID");
                // Transfer reference: prefer TransactionID, else the provider's ReferenceID/VendorID.
                result.TransactionID = FirstNonEmpty(Str(map, "TransactionID"), Str(map, "ReferenceID"));
                result.VendorID = Str(map, "VendorID");
                result.Mobile = Str(map, "Mobile");
                result.OpeningBalance = Dec(map, "OpeningBalance");
                result.Amount = Dec(map, "Amount");
                result.ClosingBalance = Dec(map, "ClosingBalance");
                // Balance enquiry: accept "Balance" and fall back to common aliases / ClosingBalance.
                result.Balance = map.ContainsKey("Balance") ? Dec(map, "Balance")
                               : map.ContainsKey("BalanceAmount") ? Dec(map, "BalanceAmount")
                               : result.ClosingBalance;
                result.Commission = Dec(map, "Commission");
                result.BeneficiaryName = Str(map, "BeneficiaryName");
                result.ConsumerNumber = Str(map, "ConsumerNumber");
                result.BankName = Str(map, "BankName");
                result.IFSC = Str(map, "IFSC");
                result.Description = Str(map, "Description");
                result.IsOTP = Bool(map, "IsOTP");
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning(ex, "JaiJui response was not valid JSON: {Body}", json);
            }

            return result;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (var v in values)
                if (!string.IsNullOrWhiteSpace(v)) return v;
            return string.Empty;
        }

        private static string Str(Dictionary<string, JsonElement> map, string name)
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

        private static decimal Dec(Dictionary<string, JsonElement> map, string name)
        {
            if (!map.TryGetValue(name, out var el)) return 0m;
            if (el.ValueKind == JsonValueKind.Number && el.TryGetDecimal(out var d)) return d;
            if (el.ValueKind == JsonValueKind.String && decimal.TryParse(el.GetString(), out var ds)) return ds;
            return 0m;
        }

        private static bool Bool(Dictionary<string, JsonElement> map, string name)
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
