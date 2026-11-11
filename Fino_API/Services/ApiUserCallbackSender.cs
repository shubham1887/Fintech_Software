using System.Security.Cryptography;
using System.Text;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.APIUserCallbackRequestResponseLog;
using FintechApp.Responses.ApiUser;
using FintechApp.StaticModels;
using Newtonsoft.Json;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IApiUserCallbackSender"/>
    /// <remarks>
    /// Wire format (default <c>Post</c> mode):
    /// <code>
    ///   POST {callbackUrl}
    ///   Content-Type: application/json
    ///   X-Request-Id / X-Transaction-Id / X-Event: CALLBACK
    ///   X-Signature: HMAC-SHA256(body, your API token), lowercase hex
    ///   { ...the same JSON contract /api/v1/recharge returns, with "event":"CALLBACK"... }
    /// </code>
    /// Delivery is retried on a non-2xx reply or a transport error (attempts and backoff are
    /// configurable). Every attempt trail is written to APIUserCallbackRequestResponseLog.
    ///
    /// Legacy receivers that were built against the old query-string GET callback can be kept
    /// working without code changes by setting <c>ApiUser:Callback:Mode</c> to <c>Get</c>
    /// (query string only) or <c>PostWithQuery</c> (JSON body AND the legacy query string).
    ///
    /// That setting is global, though, and one integrator can run more than one receiver: a panel
    /// may take prepaid/DTH on a query-string GET endpoint and other services on a JSON POST one.
    /// No single mode satisfies both, so a receiver that answers <c>405 Method Not Allowed</c> (or
    /// <c>415 Unsupported Media Type</c>) is believed rather than retried the same way three times:
    /// the reply's <c>Allow</c> header says which method it does accept, and the next attempt goes
    /// out that way immediately. A method mismatch does not heal by waiting, so that one retry
    /// skips the backoff. The switch is recorded in the attempt trail.
    /// </remarks>
    public class ApiUserCallbackSender : IApiUserCallbackSender
    {
        // Delivery modes for ApiUser:Callback:Mode.
        private const string ModePost = "POST";
        private const string ModeGet = "GET";
        private const string ModePostWithQuery = "POSTWITHQUERY";

        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICallbackMasterService _callbackService;
        private readonly IAPIUserCallbackRequestResponseLogService _callbackLog;
        private readonly IApiUserResponseBuilder _responseBuilder;
        private readonly IHttpClientFactory _httpFactory;
        private readonly Logger _logger = new();

        private readonly string _mode;
        private readonly int _timeoutSeconds;
        private readonly int _maxAttempts;
        private readonly int _retryDelaySeconds;
        private readonly bool _signEnabled;

        public ApiUserCallbackSender(
            ITransactionMasterRepository txnRepo,
            IUserRepository userRepo,
            ICallbackMasterService callbackService,
            IAPIUserCallbackRequestResponseLogService callbackLog,
            IApiUserResponseBuilder responseBuilder,
            IHttpClientFactory httpFactory,
            IConfiguration configuration)
        {
            _txnRepo = txnRepo;
            _userRepo = userRepo;
            _callbackService = callbackService;
            _callbackLog = callbackLog;
            _responseBuilder = responseBuilder;
            _httpFactory = httpFactory;

            var cfg = configuration.GetSection("ApiUser:Callback");
            _mode = (cfg.GetValue("Mode", ModePost) ?? ModePost).Trim().ToUpperInvariant();
            _timeoutSeconds = Math.Clamp(cfg.GetValue("TimeoutSeconds", 15), 3, 60);
            _maxAttempts = Math.Clamp(cfg.GetValue("MaxAttempts", 3), 1, 5);
            _retryDelaySeconds = Math.Clamp(cfg.GetValue("RetryDelaySeconds", 5), 1, 60);
            _signEnabled = cfg.GetValue("Sign", true);
        }

        public async Task Send(int transactionMasterID)
        {
            try
            {
                if (transactionMasterID <= 0) return;

                var txn = await _txnRepo.GetByID(transactionMasterID);
                if (txn == null) return;

                // Not a B2B transaction, or the caller sent no request id — nothing to correlate on.
                if (string.IsNullOrWhiteSpace(txn.APIUserRequestID)) return;

                // Only a settled transaction is worth pushing; PENDING is re-pushed when it settles.
                if (!ApiUserStatus.IsFinal(txn.Status)) return;

                var callbacks = await _callbackService.GetByUserID(txn.UserID);
                var target = callbacks.FirstOrDefault(c =>
                    c.ServiceID == txn.ServiceID && !string.IsNullOrWhiteSpace(c.CallbackURL));
                if (target?.CallbackURL == null) return;

                var payload = await _responseBuilder.FromTransaction(txn, ApiUserEventType.Callback);
                string body = JsonConvert.SerializeObject(payload, Formatting.None);
                string configuredUrl = target.CallbackURL.Trim();
                string url = BuildUrl(configuredUrl, payload);
                // Held ready in case the receiver turns out to want a GET: the query string is the
                // only way it gets the outcome, since a GET carries no body.
                string queryUrl = AppendLegacyQuery(configuredUrl, payload);

                string token = (await _userRepo.GetUserById(txn.UserID))?.APIToken ?? string.Empty;
                var (outcome, delivered, sent) = await Deliver(url, queryUrl, body, payload, token);

                await Log(txn.UserID, txn.TransactionMasterID, payload.RequestId, txn.ConsumerNumber,
                    request: sent,
                    response: outcome);

                if (!delivered)
                    _logger.Write(new Exception(
                        $"Callback not acknowledged for transaction {txn.TransactionMasterID} (user {txn.UserID}). {outcome}"));
            }
            catch (Exception ex) { _logger.Write(ex); }
        }

        /// <summary>
        /// Appends the legacy query string in the modes that keep it. The legacy parameter set is
        /// frozen at what the original GET callback sent (Status is SUCCESS/FAILURE there, not the
        /// contract's FAILED) — receivers written against it must keep working unchanged.
        /// </summary>
        private string BuildUrl(string callbackUrl, ApiUserTransactionResponse payload)
        {
            // Only the two legacy-compatible modes carry it. Anything else — including a typo in
            // the setting — falls back to the clean contract URL rather than a hybrid nobody asked for.
            if (_mode != ModeGet && _mode != ModePostWithQuery) return callbackUrl;
            return AppendLegacyQuery(callbackUrl, payload);
        }

        /// <summary>
        /// The legacy query string, appended whatever the configured mode is. Kept apart from
        /// <see cref="BuildUrl"/> because a POST that comes back 405 still needs this form to fall
        /// back to.
        /// </summary>
        private static string AppendLegacyQuery(string callbackUrl, ApiUserTransactionResponse payload)
        {
            static string E(string? v) => Uri.EscapeDataString(v ?? string.Empty);
            string legacyStatus = payload.Status == ApiUserStatus.Success ? "SUCCESS" : "FAILURE";
            string query =
                $"RequestID={E(payload.RequestId)}&ReferenceNo={E(payload.ProviderRef)}&Status={legacyStatus}" +
                $"&Number={E(payload.Number)}&Amount={payload.Amount:0.##}&Balance={payload.Balance:0.##}";

            return callbackUrl + (callbackUrl.Contains('?') ? "&" : "?") + query;
        }

        /// <summary>
        /// What a 405/415 reply tells us to do instead, or <c>null</c> when the reply says nothing
        /// useful and the attempt should simply be retried as it was. The <c>Allow</c> header is
        /// the receiver's own answer, so it is trusted over any guess; when it is missing, a
        /// rejected POST is still worth one try as the query-string GET, because that is the shape
        /// every legacy receiver of this callback was built for.
        /// </summary>
        private static HttpMethod? MethodTheReceiverWants(HttpResponseMessage response, HttpMethod sent)
        {
            if (response.StatusCode is not (System.Net.HttpStatusCode.MethodNotAllowed
                                         or System.Net.HttpStatusCode.UnsupportedMediaType))
                return null;

            var allowed = response.Content.Headers.Allow.Count > 0
                ? response.Content.Headers.Allow
                : (ICollection<string>)response.Headers
                    .Where(h => h.Key.Equals("Allow", StringComparison.OrdinalIgnoreCase))
                    .SelectMany(h => h.Value)
                    .SelectMany(v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    .ToList();

            bool Allows(string verb) => allowed.Any(a => a.Equals(verb, StringComparison.OrdinalIgnoreCase));

            if (sent == HttpMethod.Post)
                return allowed.Count == 0 || Allows("GET") ? HttpMethod.Get : null;

            return Allows("POST") ? HttpMethod.Post : null;
        }

        /// <summary>
        /// Sends the callback, retrying a transport error or a non-2xx reply. Returns the attempt
        /// trail to log and whether the integrator acknowledged with a 2xx.
        /// </summary>
        private async Task<(string outcome, bool delivered, string sent)> Deliver(
            string url, string queryUrl, string body, ApiUserTransactionResponse payload, string apiToken)
        {
            var trail = new StringBuilder();

            // What we are sending right now. It starts from the configured mode and may be changed
            // once, by the receiver itself, if it rejects the method with a 405/415.
            var method = _mode == ModeGet ? HttpMethod.Get : HttpMethod.Post;
            string target = _mode == ModeGet ? queryUrl : url;
            bool switched = false;
            string sent = Describe(method, target, body);

            for (int attempt = 1; attempt <= _maxAttempts; attempt++)
            {
                bool skipBackoff = false;
                try
                {
                    var client = _httpFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(_timeoutSeconds);

                    using var request = new HttpRequestMessage(method, target);

                    // A GET has no body, so the outcome has to ride in the query string.
                    if (method != HttpMethod.Get)
                        request.Content = new StringContent(body, Encoding.UTF8, "application/json");

                    request.Headers.TryAddWithoutValidation("X-Event", payload.Event);
                    request.Headers.TryAddWithoutValidation("X-Request-Id", payload.RequestId);
                    request.Headers.TryAddWithoutValidation("X-Transaction-Id", payload.TxnId.ToString());
                    request.Headers.TryAddWithoutValidation("X-Attempt", $"{attempt}/{_maxAttempts}");

                    // Lets the integrator prove the callback came from us and was not tampered
                    // with, without exposing the token itself. Key = their own API token.
                    if (_signEnabled && method != HttpMethod.Get && apiToken.Length > 0)
                    {
                        request.Headers.TryAddWithoutValidation("X-Signature-Algorithm", "HMAC-SHA256");
                        request.Headers.TryAddWithoutValidation("X-Signature", Sign(body, apiToken));
                    }

                    sent = Describe(method, target, body);

                    using var response = await client.SendAsync(request);
                    string replyBody = await response.Content.ReadAsStringAsync();
                    if (replyBody.Length > 1000) replyBody = replyBody.Substring(0, 1000);

                    trail.Append($"[attempt {attempt}/{_maxAttempts}] {method.Method} HTTP {(int)response.StatusCode} {replyBody}");

                    if (response.IsSuccessStatusCode) return (trail.ToString(), true, sent);

                    // The receiver says the method is wrong. Believe it once: a second identical
                    // attempt can only earn a second 405.
                    if (!switched && MethodTheReceiverWants(response, method) is HttpMethod wanted)
                    {
                        method = wanted;
                        target = wanted == HttpMethod.Get ? queryUrl : url;
                        switched = true;
                        skipBackoff = true;
                        trail.Append($" -> receiver accepts {wanted.Method}, retrying as {wanted.Method}");
                    }
                }
                catch (Exception ex)
                {
                    trail.Append($"[attempt {attempt}/{_maxAttempts}] {method.Method} ERROR: {ex.Message}");
                }

                if (attempt < _maxAttempts)
                {
                    trail.Append('\n');
                    // Linear backoff — a receiver that is briefly down gets a widening window,
                    // and the total wait stays bounded and predictable for the operations team.
                    // A method mismatch is not a transient fault, so that retry goes out at once.
                    if (!skipBackoff)
                        await Task.Delay(TimeSpan.FromSeconds(_retryDelaySeconds * attempt));
                }
            }

            return (trail.ToString(), false, sent);
        }

        private static string Describe(HttpMethod method, string url, string body) =>
            method == HttpMethod.Get ? $"GET {url}" : $"{method.Method} {url}\n{body}";

        private static string Sign(string body, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(body))).ToLowerInvariant();
        }

        private async Task Log(int userId, int txnId, string requestId, string consumerNumber, string request, string response)
        {
            try
            {
                await _callbackLog.Create(new CreateAPIUserCallbackRequestResponseLogRequest
                {
                    UserID = userId,
                    TransactionMasterID = txnId,
                    APIUserRequestID = requestId,
                    ConsumerNumber = consumerNumber,
                    Request = request,
                    Response = response,
                });
            }
            catch (Exception ex) { _logger.Write(ex); }
        }
    }
}
