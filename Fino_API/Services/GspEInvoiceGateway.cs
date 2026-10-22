using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;
using Newtonsoft.Json.Linq;

namespace FintechApp.Services
{
    /// <summary>
    /// The real Invoice Registration Portal gateway, over HTTP to the install's GSP (or NIC).
    ///
    /// It is deliberately configuration-driven rather than wired to one GSP, because this platform
    /// is resold and each client brings its own. What is common to every GSP is the shape: an
    /// optional auth call that returns a session token, then a POST of the NIC invoice JSON that
    /// comes back with an IRN or an error. The endpoints, credentials and timeouts are in
    /// appsettings "EInvoice"; the field NAMES in the response vary, so the parser searches the
    /// body for the IRN rather than assuming one layout — a success is a body that carries an Irn,
    /// anything else is the portal's error.
    ///
    /// It never pretends. A transport failure or a 5xx is retryable (the document stays Pending and
    /// is tried again); a portal rejection on the document's own merits is permanent (retrying an
    /// invalid payload forever only hides it). This is exactly the contract the queue relies on.
    /// </summary>
    public class GspEInvoiceGateway : IEInvoiceGateway
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly EInvoiceSettings _cfg;
        private readonly ILogger<GspEInvoiceGateway> _log;

        // Auth token cached across requests (and across scoped instances) so one token serves many
        // documents. Keyed by GSTIN, guarded by a lock — the sweep can process in parallel.
        private static readonly object _tokenLock = new();
        private static string _token = string.Empty;
        private static string _tokenGstin = string.Empty;
        private static DateTime _tokenExpiry = DateTime.MinValue;

        public GspEInvoiceGateway(IHttpClientFactory httpFactory, EInvoiceSettings cfg, ILogger<GspEInvoiceGateway> log)
        {
            _httpFactory = httpFactory;
            _cfg = cfg;
            _log = log;
        }

        public bool IsConfigured => _cfg.IsConfigured;

        public async Task<EInvoiceGatewayResult> Register(string requestPayloadJson)
        {
            if (!IsConfigured)
                return Fail("NOT_CONFIGURED", "The e-invoice gateway is not configured.", retryable: true);

            try
            {
                var result = await Post(requestPayloadJson);

                // A token that has expired server-side shows up as a 401 even though ours looks
                // valid. Clear it and try exactly once more, so a stale token costs one retry
                // rather than failing every document until the cache ages out.
                if (result.ErrorCode == "401")
                {
                    ClearToken();
                    result = await Post(requestPayloadJson);
                }
                return result;
            }
            catch (TaskCanceledException)
            {
                return Fail("TIMEOUT", $"The IRP did not respond within {_cfg.TimeoutSeconds}s.", retryable: true);
            }
            catch (HttpRequestException ex)
            {
                _log.LogWarning(ex, "e-invoice: transport error reaching the IRP.");
                return Fail("TRANSPORT", "Could not reach the IRP: " + ex.Message, retryable: true);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "e-invoice: unexpected error registering a document.");
                return Fail("ERROR", ex.Message, retryable: true);
            }
        }

        private async Task<EInvoiceGatewayResult> Post(string payloadJson)
        {
            var client = _httpFactory.CreateClient("einvoice");
            client.Timeout = TimeSpan.FromSeconds(_cfg.TimeoutSeconds);

            string token = await EnsureToken(client);

            using var req = new HttpRequestMessage(HttpMethod.Post, Combine(_cfg.BaseUrl, _cfg.GeneratePath));
            // The headers every GSP wants in some casing; sending the common set covers the usual
            // layouts without a per-GSP build.
            AddHeader(req, "Gstin", _cfg.Gstin);
            AddHeader(req, "client-id", _cfg.ClientId);
            AddHeader(req, "client-secret", _cfg.ClientSecret);
            AddHeader(req, "user_name", _cfg.Username);
            if (!string.IsNullOrEmpty(token))
            {
                AddHeader(req, "AuthToken", token);
                req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
            }
            req.Content = new StringContent(payloadJson, System.Text.Encoding.UTF8, "application/json");

            using var resp = await client.SendAsync(req);
            string body = await resp.Content.ReadAsStringAsync();

            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return Fail("401", "The IRP rejected the session token.", retryable: true, raw: body);

            if (!resp.IsSuccessStatusCode)
            {
                // 5xx / 408 / 429 are the portal's problem and worth retrying; a 4xx is the
                // document's and is not.
                int code = (int)resp.StatusCode;
                bool retry = code >= 500 || code == 408 || code == 429;
                return Fail(code.ToString(), $"The IRP returned HTTP {code}. {Trim(body)}", retry, raw: body);
            }

            return Parse(body);
        }

        /// <summary>A reply is a success when it carries an IRN anywhere in the body; otherwise it
        /// is the portal's own error, which is permanent (the payload has to be corrected).</summary>
        private EInvoiceGatewayResult Parse(string body)
        {
            JToken root;
            try { root = JToken.Parse(body); }
            catch { return Fail("BAD_RESPONSE", "The IRP response was not JSON: " + Trim(body), retryable: true, raw: body); }

            string irn = FindValue(root, "Irn") ?? FindValue(root, "irn") ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(irn))
            {
                return new EInvoiceGatewayResult
                {
                    Success = true,
                    IRN = irn,
                    AckNo = FindValue(root, "AckNo") ?? FindValue(root, "ackNo") ?? string.Empty,
                    AckDate = TryDate(FindValue(root, "AckDt") ?? FindValue(root, "ackDt")),
                    SignedQRCode = FindValue(root, "SignedQRCode") ?? FindValue(root, "signedQRCode") ?? string.Empty,
                    RawResponse = body,
                };
            }

            // No IRN — read whatever error the portal gave. ErrorDetails is the NIC shape; message /
            // ErrorMessage cover the common GSP wrappers.
            string err = FindValue(root, "ErrorDetails")
                      ?? FindValue(root, "ErrorMessage")
                      ?? FindValue(root, "message")
                      ?? FindValue(root, "error")
                      ?? "The IRP returned no IRN and no error detail.";
            string errCode = FindValue(root, "ErrorCode") ?? FindValue(root, "errorCode") ?? "REJECTED";

            // A duplicate IRN is not really a failure — the document is already registered. The
            // caller keeps it Pending and the operator can pull the existing IRN; flag it clearly.
            bool duplicate = err.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                          || errCode is "2150" or "2172";
            return Fail(errCode, Trim(err), retryable: duplicate, raw: body);
        }

        private async Task<string> EnsureToken(HttpClient client)
        {
            if (string.IsNullOrWhiteSpace(_cfg.AuthPath)) return string.Empty; // creds ride as headers

            lock (_tokenLock)
            {
                if (_tokenGstin == _cfg.Gstin && _tokenExpiry > DateTime.UtcNow && !string.IsNullOrEmpty(_token))
                    return _token;
            }

            var authBody = new JObject
            {
                ["ClientId"] = _cfg.ClientId,
                ["ClientSecret"] = _cfg.ClientSecret,
                ["Gstin"] = _cfg.Gstin,
                ["UserName"] = _cfg.Username,
                ["Password"] = _cfg.Password,
            };
            using var req = new HttpRequestMessage(HttpMethod.Post, Combine(_cfg.BaseUrl, _cfg.AuthPath))
            {
                Content = new StringContent(authBody.ToString(), System.Text.Encoding.UTF8, "application/json"),
            };
            AddHeader(req, "client-id", _cfg.ClientId);
            AddHeader(req, "client-secret", _cfg.ClientSecret);

            using var resp = await client.SendAsync(req);
            string body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                _log.LogWarning("e-invoice: auth call returned HTTP {Code}. {Body}", (int)resp.StatusCode, Trim(body));
                return string.Empty; // Post() will try unauthenticated; the portal decides.
            }

            JToken root;
            try { root = JToken.Parse(body); } catch { return string.Empty; }
            string token = FindValue(root, "AuthToken") ?? FindValue(root, "authToken")
                        ?? FindValue(root, "token") ?? FindValue(root, "access_token") ?? string.Empty;

            if (!string.IsNullOrEmpty(token))
            {
                lock (_tokenLock)
                {
                    _token = token;
                    _tokenGstin = _cfg.Gstin;
                    _tokenExpiry = DateTime.UtcNow.AddHours(Math.Max(1, _cfg.AuthTokenHours));
                }
            }
            return token;
        }

        private static void ClearToken()
        {
            lock (_tokenLock) { _token = string.Empty; _tokenExpiry = DateTime.MinValue; }
        }

        // ── helpers ──────────────────────────────────────────────────────────────────────
        private static void AddHeader(HttpRequestMessage req, string name, string value)
        {
            if (!string.IsNullOrEmpty(value)) req.Headers.TryAddWithoutValidation(name, value);
        }

        private static string Combine(string baseUrl, string path) =>
            baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');

        /// <summary>First value for a property with this name anywhere in the tree (case-sensitive;
        /// callers try both casings). GSPs nest the useful fields under "Data" or "result", so a
        /// recursive search is more robust than a fixed path.</summary>
        private static string? FindValue(JToken node, string name)
        {
            if (node is JObject obj)
            {
                foreach (var p in obj.Properties())
                {
                    if (p.Name == name && (p.Value.Type == JTokenType.String || p.Value.Type == JTokenType.Integer
                                        || p.Value.Type == JTokenType.Float || p.Value.Type == JTokenType.Date))
                        return p.Value.ToString();
                    var nested = FindValue(p.Value, name);
                    if (nested != null) return nested;
                }
            }
            else if (node is JArray arr)
            {
                foreach (var item in arr)
                {
                    var nested = FindValue(item, name);
                    if (nested != null) return nested;
                }
            }
            return null;
        }

        private static DateTime? TryDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            foreach (var fmt in new[] { "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy HH:mm:ss", "yyyy-MM-ddTHH:mm:ss" })
                if (DateTime.TryParseExact(s, fmt, null, System.Globalization.DateTimeStyles.None, out var d)) return d;
            return DateTime.TryParse(s, out var g) ? g : null;
        }

        private static string Trim(string s) => string.IsNullOrEmpty(s) ? s : (s.Length > 500 ? s[..500] : s);

        private static EInvoiceGatewayResult Fail(string code, string message, bool retryable, string raw = "") => new()
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message,
            IsRetryable = retryable,
            RawResponse = raw,
        };
    }
}
