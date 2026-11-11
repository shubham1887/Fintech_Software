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

            var (body, reached, failure) = await Call(options, OpBillerInfo, NewRequestID());
            if (!reached) return Pay1BillerResponse.Fail(failure);

            return ParseBillerInfo(body);
        }

        public async Task<Pay1ApiResponse> BillFetch(BillFetchRequest request, Pay1Credentials? creds = null)
        {
            if (string.IsNullOrWhiteSpace(request.Operator)) return Pay1ApiResponse.Fail(Pay1.Message.OperatorRequired);
            if (string.IsNullOrWhiteSpace(request.AccountNumber)) return Pay1ApiResponse.Fail(Pay1.Message.NumberRequired);

            var (options, error) = Resolve(creds);
            if (error != null) return Pay1ApiResponse.Fail(error);

            string transId = string.IsNullOrWhiteSpace(request.RequestID) ? NewRequestID() : request.RequestID.Trim();
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
            var (body, reached, failure) = await Call(options, OpBalance, NewRequestID());
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

        private const int SequenceRange = 10000;

        // Rolling tail for NewRequestID, started at a random point so two instances of the host do
        // not march through the same sequence in the same second.
        private static int _sequence = Random.Shared.Next(0, SequenceRange);

        /// <summary>Unique reference for the calls that have to invent one (biller list, balance,
        /// bill fetch). Digits only, deliberately: the document asks for a "random number" on the
        /// balance enquiry and every trans_id it samples is numeric, so a letter-prefixed id risks
        /// being rejected upstream as malformed.
        ///
        /// The tail counts rather than rolls dice. Three random digits gave only 900 ids a second,
        /// which collides about a quarter of the time at even a modest bill-fetch rate, and Pay1
        /// rejects a repeated trans_id. Counting makes the first 10,000 calls in any one second
        /// collision-free, which is far past what this platform will ever ask of it.</summary>
        public static string NewRequestID()
        {
            int tail = (int)((uint)Interlocked.Increment(ref _sequence) % SequenceRange);
            return DateTime.Now.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture) +
                   tail.ToString("D4", CultureInfo.InvariantCulture);
        }

        // ── Response parsing ─────────────────────────────────────────────────────────

        // One parser for every operation: the envelope is always the same (status / errCode /
        // description), and the operation-specific fields sit either beside it (payment, balance)
        // or inside "description" (bill fetch).
        //
        // A transaction enquiry (operation_type 3) answers with one object PER REQUEST ID instead
        // of the single object every other operation returns, and it arrives in one of two shapes.
        // The live API nests the list under "status":
        //
        //     {"status":[{"req_id":"…","status":"failure","errCode":"E015","trans_id":""}]}
        //
        // which makes "status" an ARRAY where every other operation has a string — read it as a
        // string and the enquiry silently yields no status at all, leaving good transactions stuck
        // pending. The document instead prints a flat object, and a bare top-level array also turns
        // up, so all three are accepted here. The entries land in Results and the first is copied
        // to the top level, which keeps a one-id enquiry identical however it was wrapped.
        private static Pay1ApiResponse ParseCommon(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return Unreadable(body);

            JsonDocument document;
            try { document = JsonDocument.Parse(body); }
            catch { return Unreadable(body); }

            using (document)
            {
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    return TryGet(root, out var nested, "status") && nested.ValueKind == JsonValueKind.Array
                        ? ParseBatch(nested, body)
                        : ParseObject(root, body);
                }

                if (root.ValueKind == JsonValueKind.Array) return ParseBatch(root, body);

                return Unreadable(body);
            }
        }

        // One entry per request id. Parsed twice on purpose: the head is its own instance, so
        // Results never contains the object that owns it.
        private static Pay1ApiResponse ParseBatch(JsonElement array, string body)
        {
            var entries = array.EnumerateArray()
                               .Where(element => element.ValueKind == JsonValueKind.Object)
                               .ToList();
            if (entries.Count == 0) return Unreadable(body);

            var head = ParseObject(entries[0], body);
            head.Results = entries.Select(element => ParseObject(element, body)).ToList();
            return head;
        }

        private static Pay1ApiResponse ParseObject(JsonElement root, string body)
        {
            var result = new Pay1ApiResponse
            {
                Data = body ?? string.Empty,
                Attempted = true,
                Success = true,
                Status = Str(root, "status").ToLowerInvariant(),
                ErrorCode = Str(root, "errCode", "err_code"),
                RequestID = Str(root, "partner_reqid", "req_id"),
                TransactionID = Str(root, "ref_code", "trans_id"),
                Balance = Dec(root, "balance"),
                ComplaintID = Str(root, "complaint_id"),
                TurnaroundTimeHours = (int)Dec(root, "turnaround_time"),
            };

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
                result.AccountLabel = Str(description, "acc_no_label");
            }
            else
            {
                result.Message = Str(root, "description", "message");
            }

            return result;
        }

        private static Pay1ApiResponse Unreadable(string? body) => new()
        {
            Data = body ?? string.Empty,
            Attempted = true,
            Success = false,
            Message = Pay1.Message.UnreadableResponse,
        };

        private static Pay1BillerResponse ParseBillerInfo(string body)
        {
            var response = new Pay1BillerResponse { Data = body ?? string.Empty };
            if (string.IsNullOrWhiteSpace(body)) return Pay1BillerResponse.Fail(Pay1.Message.UnreadableResponse, body ?? string.Empty);

            JsonDocument document;
            try { document = JsonDocument.Parse(body); }
            catch { return Pay1BillerResponse.Fail(Pay1.Message.UnreadableResponse, body); }

            using (document)
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return Pay1BillerResponse.Fail(Pay1.Message.UnreadableResponse, body);

                if (!Str(root, "status").Equals("success", StringComparison.OrdinalIgnoreCase))
                {
                    string message = TryGet(root, out var d, "description") && d.ValueKind == JsonValueKind.String
                        ? d.GetString() ?? string.Empty
                        : Pay1.Message.ServiceUnavailable;
                    return Pay1BillerResponse.Fail(message, body);
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
                    return Pay1BillerResponse.Fail(Pay1.Message.UnreadableResponse, body);
                }

                foreach (var item in data.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;

                    var biller = new Pay1Biller
                    {
                        ProductID = Str(item, "product_id"),
                        Name = Str(item, "Name", "name", "biller_name"),
                        Category = Str(item, "category"),
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
