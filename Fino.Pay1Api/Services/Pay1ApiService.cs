using System.Globalization;
using System.Text;
using System.Text.Json;
using Fino.Pay1Api.Configuration;
using Fino.Pay1Api.Interfaces;
using Fino.Pay1Api.Messages;
using Fino.Pay1Api.Requests;
using Fino.Pay1Api.Responses;
using Fino.Pay1Api.Security;
using Microsoft.Extensions.Configuration;

namespace Fino.Pay1Api.Services
{
    // Builds recharges.pay1.in receiveApi URLs, signs each one with
    // hash_code = sha1(partner_id + trans_id + api_key), delegates the HTTP GET to IPay1ApiClient
    // and parses the reply. Endpoint and parameter names match the Pay1 Utility API document
    // exactly. Credentials come from the per-call override (the provider row serving the biller)
    // when supplied, otherwise from the "Pay1Api" configuration section.
    public class Pay1ApiService : IPay1ApiService
    {
        // operation_type values from the provider document.
        private const int OpBalance = 2;
        private const int OpStatusCheck = 3;
        private const int OpComplaint = 4;
        private const int OpComplaintStatus = 5;
        private const int OpBillerInfo = 7;
        private const int OpBillFetch = 8;
        private const int OpBillPayment = 9;

        private const int MaxStatusIDsPerCall = 10;

        private readonly IPay1ApiClient _client;
        private readonly IConfiguration _config;

        public Pay1ApiService(IPay1ApiClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
        }

        // ── Operations ───────────────────────────────────────────────────────────────

        public async Task<Pay1BillerResponse> BillerInfo(Pay1Credentials? creds = null)
        {
            var (options, error) = Resolve(creds);
            if (error != null) return Pay1BillerResponse.Fail(error);

            var (body, reached, failure) = await Call(options, OpBillerInfo, NewRequestID("BI"));
            if (!reached) return Pay1BillerResponse.Fail(failure);

            return ParseBillerInfo(body);
        }

        public async Task<Pay1ApiResponse> BillFetch(BillFetchRequest request, Pay1Credentials? creds = null)
        {
            if (string.IsNullOrWhiteSpace(request.Operator)) return Pay1ApiResponse.Fail(Pay1.Message.OperatorRequired);
            if (string.IsNullOrWhiteSpace(request.AccountNumber)) return Pay1ApiResponse.Fail(Pay1.Message.NumberRequired);

            var (options, error) = Resolve(creds);
            if (error != null) return Pay1ApiResponse.Fail(error);

            string transId = string.IsNullOrWhiteSpace(request.RequestID) ? NewRequestID("BF") : request.RequestID.Trim();
            var (body, reached, failure) = await Call(options, OpBillFetch, transId, new Dictionary<string, string>
            {
                ["operator"] = request.Operator.Trim(),
                ["accountNumber"] = request.AccountNumber.Trim(),
                ["param"] = request.Param ?? string.Empty,
                ["param1"] = request.Param1 ?? string.Empty,
            });
            if (!reached) return Transport(failure);

            var result = ParseCommon(body);
            if (result.IsFailure && result.Message.Length == 0) result.Message = Pay1.Message.NoBillDue;
            if (result.IsSuccess && result.Message.Length == 0) result.Message = Pay1.Message.BillFetched;
            return result;
        }

        public async Task<Pay1ApiResponse> BillPayment(BillPaymentRequest request, Pay1Credentials? creds = null)
        {
            if (string.IsNullOrWhiteSpace(request.Operator)) return Pay1ApiResponse.Fail(Pay1.Message.OperatorRequired);
            if (string.IsNullOrWhiteSpace(request.AccountNumber)) return Pay1ApiResponse.Fail(Pay1.Message.NumberRequired);
            if (request.Amount <= 0) return Pay1ApiResponse.Fail(Pay1.Message.AmountRequired);
            if (string.IsNullOrWhiteSpace(request.RequestID)) return Pay1ApiResponse.Fail(Pay1.Message.RequestIDRequired);

            var (options, error) = Resolve(creds);
            if (error != null) return Pay1ApiResponse.Fail(error);

            var (body, reached, failure) = await Call(options, OpBillPayment, request.RequestID.Trim(), new Dictionary<string, string>
            {
                ["operator"] = request.Operator.Trim(),
                ["amount"] = request.Amount.ToString("0.##", CultureInfo.InvariantCulture),
                ["accountNumber"] = request.AccountNumber.Trim(),
                ["param"] = request.Param ?? string.Empty,
                ["param1"] = request.Param1 ?? string.Empty,
            });
            if (!reached) return Transport(failure);

            return ParseCommon(body);
        }

        public async Task<Pay1ApiResponse> Balance(Pay1Credentials? creds = null)
        {
            var (options, error) = Resolve(creds);
            if (error != null) return Pay1ApiResponse.Fail(error);

            // The balance enquiry is signed over a random reference rather than a transaction id.
            var (body, reached, failure) = await Call(options, OpBalance, NewRequestID("BAL"));
            if (!reached) return Transport(failure);

            var result = ParseCommon(body);
            if (result.IsSuccess && result.Message.Length == 0) result.Message = Pay1.Message.BalanceFetched;
            return result;
        }

        public async Task<Pay1ApiResponse> StatusCheck(StatusCheckRequest request, Pay1Credentials? creds = null)
        {
            string ids = string.Join(',', (request.RequestIDs ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(MaxStatusIDsPerCall));
            if (ids.Length == 0) return Pay1ApiResponse.Fail(Pay1.Message.RequestIDRequired);

            var (options, error) = Resolve(creds);
            if (error != null) return Pay1ApiResponse.Fail(error);

            var (body, reached, failure) = await Call(options, OpStatusCheck, ids);
            if (!reached) return Transport(failure);

            return ParseCommon(body);
        }

        public async Task<Pay1ApiResponse> RaiseComplaint(ComplaintRequest request, Pay1Credentials? creds = null)
        {
            if (string.IsNullOrWhiteSpace(request.TransactionID)) return Pay1ApiResponse.Fail(Pay1.Message.RequestIDRequired);

            var (options, error) = Resolve(creds);
            if (error != null) return Pay1ApiResponse.Fail(error);

            string tag = string.IsNullOrWhiteSpace(request.TagID) ? Pay1ComplaintTag.SuccessfulNotUpdated : request.TagID.Trim();
            var (body, reached, failure) = await Call(options, OpComplaint, request.TransactionID.Trim(),
                new Dictionary<string, string> { ["tag_id"] = tag });
            if (!reached) return Transport(failure);

            return ParseCommon(body);
        }

        public async Task<Pay1ApiResponse> ComplaintStatus(string complaintID, Pay1Credentials? creds = null)
        {
            if (string.IsNullOrWhiteSpace(complaintID)) return Pay1ApiResponse.Fail(Pay1.Message.ComplaintIDRequired);

            var (options, error) = Resolve(creds);
            if (error != null) return Pay1ApiResponse.Fail(error);

            var (body, reached, failure) = await Call(options, OpComplaintStatus, complaintID.Trim());
            if (!reached) return Transport(failure);

            return ParseCommon(body);
        }

        // ── Inbound status push ──────────────────────────────────────────────────────

        public Pay1CallbackResult ParseCallback(IDictionary<string, string> values, string raw, Pay1Credentials? creds = null)
        {
            var result = new Pay1CallbackResult { Raw = raw ?? string.Empty };
            string Read(string key) => values != null && values.TryGetValue(key, out var v) ? (v ?? string.Empty).Trim() : string.Empty;

            result.RequestID = Read("client_req_id");
            result.TransactionID = Read("trans_id");
            result.OperatorID = Read("operator_id");
            result.Status = Read("status");
            result.ErrorCode = Read("err_code");
            result.Description = Read("description");
            result.PartnerID = Read("partner_id");
            result.ClosingBalance = Read("close_bal");
            result.IsReversal = Read("reversal_status") is "1" or "true";

            var (options, error) = Resolve(creds);
            if (error != null)
            {
                result.Message = error;
                return result;
            }

            // Both the signature AND the account must line up before the host settles anything.
            if (!string.Equals(options.PartnerId, result.PartnerID, StringComparison.OrdinalIgnoreCase))
            {
                result.Message = "Callback rejected: partner id does not match the configured account.";
                return result;
            }
            if (!Pay1Signature.Verify(options.PartnerId, result.RequestID, options.ApiKey, Read("hash_code")))
            {
                result.Message = "Callback rejected: signature mismatch.";
                return result;
            }

            result.Verified = true;
            result.Message = "Verified.";
            return result;
        }

        // ── URL building + credentials ───────────────────────────────────────────────

        // Effective options: the "Pay1Api" config section, overlaid with any non-blank per-call value.
        private (Pay1ApiOptions options, string? error) Resolve(Pay1Credentials? creds)
        {
            var o = new Pay1ApiOptions();
            _config.GetSection(Pay1ApiOptions.SectionName).Bind(o);

            if (creds != null)
            {
                if (!string.IsNullOrWhiteSpace(creds.BaseUrl)) o.BaseUrl = creds.BaseUrl;
                if (!string.IsNullOrWhiteSpace(creds.PartnerId)) o.PartnerId = creds.PartnerId;
                if (!string.IsNullOrWhiteSpace(creds.ApiKey)) o.ApiKey = creds.ApiKey;
            }

            if (string.IsNullOrWhiteSpace(o.BaseUrl)) o.BaseUrl = "https://recharges.pay1.in";
            o.PartnerId = (o.PartnerId ?? string.Empty).Trim();
            o.ApiKey = (o.ApiKey ?? string.Empty).Trim();

            if (o.PartnerId.Length == 0 || o.ApiKey.Length == 0)
                return (o, Pay1.Message.CredentialsMissing);

            return (o, null);
        }

        // A failure AFTER the request left our server. Attempted=true tells the caller it must
        // hold the transaction rather than declare it failed.
        private static Pay1ApiResponse Transport(string message) =>
            new() { Success = false, Attempted = true, Message = message };

        private Task<(string body, bool reached, string failureMessage)> Call(
            Pay1ApiOptions options, int operationType, string transId, IDictionary<string, string>? extra = null)
        {
            return _client.Get(BuildUrl(options, operationType, transId, extra), options.TimeoutSeconds);
        }

        internal static string BuildUrl(Pay1ApiOptions options, int operationType, string transId,
            IDictionary<string, string>? extra = null)
        {
            var query = new StringBuilder();
            void Add(string key, string value) =>
                query.Append(query.Length == 0 ? '?' : '&')
                     .Append(key).Append('=')
                     .Append(Uri.EscapeDataString(value ?? string.Empty));

            Add("partner_id", options.PartnerId);
            Add("operation_type", operationType.ToString(CultureInfo.InvariantCulture));
            Add("trans_id", transId);
            if (extra != null)
            {
                foreach (var pair in extra) Add(pair.Key, pair.Value);
            }
            Add("hash_code", Pay1Signature.Compute(options.PartnerId, transId, options.ApiKey));

            return EndpointRoot(options.BaseUrl) + query;
        }

        private static string EndpointRoot(string baseUrl)
        {
            string root = (baseUrl ?? string.Empty).Trim();
            if (root.Contains("receiveApi", StringComparison.OrdinalIgnoreCase)) return root.TrimEnd('?', '&');
            return root.TrimEnd('/') + "/apis/receiveApi";
        }

        /// <summary>Unique numeric reference for the calls that need one of their own.</summary>
        public static string NewRequestID(string prefix = "") =>
            prefix + DateTime.Now.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture) + Random.Shared.Next(100, 999);

        // ── Response parsing ─────────────────────────────────────────────────────────

        // One parser for every operation: the envelope is always the same (status / errCode /
        // description), and the operation-specific fields sit either beside it (payment, balance)
        // or inside "description" (bill fetch).
        private static Pay1ApiResponse ParseCommon(string body)
        {
            var result = new Pay1ApiResponse { Data = body ?? string.Empty, Attempted = true };
            if (string.IsNullOrWhiteSpace(body)) return Transport(Pay1.Message.UnreadableResponse);

            JsonDocument document;
            try { document = JsonDocument.Parse(body); }
            catch { return new Pay1ApiResponse { Data = body, Attempted = true, Success = false, Message = Pay1.Message.UnreadableResponse }; }

            using (document)
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    return new Pay1ApiResponse { Data = body, Attempted = true, Success = false, Message = Pay1.Message.UnreadableResponse };

                result.Success = true;
                result.Status = Str(root, "status").ToLowerInvariant();
                result.ErrorCode = Str(root, "errCode", "err_code");
                result.RequestID = Str(root, "partner_reqid", "req_id");
                result.TransactionID = Str(root, "ref_code", "trans_id");
                result.Balance = Dec(root, "balance");
                result.ComplaintID = Str(root, "complaint_id");
                result.TurnaroundTimeHours = (int)Dec(root, "turnaround_time");

                // "description" is a message on most operations and an object on bill fetch.
                if (TryGet(root, out var description, "description") && description.ValueKind == JsonValueKind.Object)
                {
                    result.BillNumber = Str(description, "bill_number");
                    result.BillDate = Str(description, "bill_date");
                    result.DueDate = Str(description, "due_date");
                    result.BillAmount = Dec(description, "bill_amount");
                    result.BillPeriod = Str(description, "bill_period");
                    result.CustomerName = Str(description, "customer_name");
                    result.MobileNumber = Str(description, "mobile_number");
                }
                else
                {
                    result.Message = Str(root, "description", "message");
                }
            }

            return result;
        }

        private static Pay1BillerResponse ParseBillerInfo(string body)
        {
            var response = new Pay1BillerResponse { Data = body ?? string.Empty };
            if (string.IsNullOrWhiteSpace(body)) return Pay1BillerResponse.Fail(Pay1.Message.UnreadableResponse);

            JsonDocument document;
            try { document = JsonDocument.Parse(body); }
            catch { return Pay1BillerResponse.Fail(Pay1.Message.UnreadableResponse); }

            using (document)
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return Pay1BillerResponse.Fail(Pay1.Message.UnreadableResponse);

                if (!Str(root, "status").Equals("success", StringComparison.OrdinalIgnoreCase))
                {
                    string message = TryGet(root, out var d, "description") && d.ValueKind == JsonValueKind.String
                        ? d.GetString() ?? string.Empty
                        : Pay1.Message.ServiceUnavailable;
                    return Pay1BillerResponse.Fail(message);
                }

                // { "status":"success", "code":200, "description": { "data": [ … ] } }
                JsonElement data;
                if (TryGet(root, out var description, "description") && description.ValueKind == JsonValueKind.Object
                    && TryGet(description, out data, "data") && data.ValueKind == JsonValueKind.Array)
                {
                    // ok
                }
                else if (TryGet(root, out data, "data") && data.ValueKind == JsonValueKind.Array)
                {
                    // some deployments return the array at the top level
                }
                else
                {
                    return Pay1BillerResponse.Fail(Pay1.Message.UnreadableResponse);
                }

                foreach (var item in data.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;

                    var biller = new Pay1Biller
                    {
                        ProductID = Str(item, "product_id"),
                        Name = Str(item, "Name", "name", "biller_name"),
                        BillFetch = Flag(item, "bill_fetch"),
                        AmountChange = Flag(item, "amount_change"),
                        AmountExactness = Str(item, "amt_exactness"),
                        AfterDueDate = Flag(item, "after_due_date"),
                    };
                    if (biller.ProductID.Length == 0 || biller.Name.Length == 0) continue;

                    // "fields": { "1": {…}, "2": {…} } — keyed by position, so order by the key.
                    if (TryGet(item, out var fields, "fields") && fields.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var property in fields.EnumerateObject()
                                     .OrderBy(p => int.TryParse(p.Name, out int n) ? n : int.MaxValue)
                                     .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
                        {
                            if (property.Value.ValueKind != JsonValueKind.Object) continue;
                            biller.Fields.Add(new Pay1BillerField
                            {
                                Label = Str(property.Value, "label"),
                                Param = Str(property.Value, "param"),
                                Sample = Str(property.Value, "sample"),
                                Regex = Str(property.Value, "regex"),
                                Input = Str(property.Value, "input"),
                            });
                        }
                    }

                    response.Billers.Add(biller);
                }
            }

            response.Success = true;
            response.Message = $"{response.Billers.Count} biller(s) returned.";
            return response;
        }

        // Property lookup that ignores case, underscores and stray whitespace in the key — the
        // provider document itself shows one reply with a trailing space in a field name.
        private static bool TryGet(JsonElement obj, out JsonElement value, params string[] names)
        {
            value = default;
            if (obj.ValueKind != JsonValueKind.Object) return false;

            foreach (string name in names)
            {
                string wanted = Normalize(name);
                foreach (var property in obj.EnumerateObject())
                {
                    if (Normalize(property.Name) != wanted) continue;
                    value = property.Value;
                    return true;
                }
            }
            return false;
        }

        private static string Normalize(string key) =>
            key.Trim().ToLowerInvariant().Replace("_", string.Empty).Replace(" ", string.Empty);

        private static string Str(JsonElement obj, params string[] names)
        {
            if (!TryGet(obj, out var value, names)) return string.Empty;
            return value.ValueKind switch
            {
                JsonValueKind.String => (value.GetString() ?? string.Empty).Trim(),
                JsonValueKind.Number => value.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => string.Empty
            };
        }

        private static decimal Dec(JsonElement obj, params string[] names)
        {
            if (!TryGet(obj, out var value, names)) return 0m;
            if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out decimal number)) return number;
            return decimal.TryParse(Str(obj, names), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed)
                ? parsed : 0m;
        }

        // Pay1 sends these as real booleans, but "1" / "true" turn up too.
        private static bool Flag(JsonElement obj, string name)
        {
            if (!TryGet(obj, out var value, name)) return false;
            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => value.TryGetInt32(out int n) && n != 0,
                JsonValueKind.String => (value.GetString() ?? string.Empty).Trim().ToLowerInvariant() is "1" or "true" or "yes",
                _ => false
            };
        }
    }
}
