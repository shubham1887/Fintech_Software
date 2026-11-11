using System.Security.Cryptography;
using System.Text;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FintechApp.Services
{
    /// <summary>
    /// FCM HTTP v1 sender, written against the BCL only (no Google SDK to restore on a
    /// customer's offline server).
    ///
    /// Two things it does that the old legacy-key sender could not:
    ///  * authenticates with a service account (the legacy server key stopped working in 2024);
    ///  * sets an Android <c>channel_id</c>. Since Android 8 a notification whose channel does
    ///    not exist is dropped without a trace - that alone would have kept the tray empty even
    ///    if the old endpoint had still been alive.
    ///
    /// Configuration (appsettings "FCM"):
    ///   ProjectId          - Firebase project id
    ///   ServiceAccountFile - path to the downloaded service-account JSON, OR
    ///   ServiceAccountJson - the same JSON inline
    /// Leave them blank and the platform still works: alerts are stored and show in the bell,
    /// only the tray notification is skipped.
    /// </summary>
    public class FcmSender : IFcmSender
    {
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        private const string Scope = "https://www.googleapis.com/auth/firebase.messaging";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Logger _logger = new();

        private readonly string _projectId;
        private readonly string _clientEmail;
        private readonly string _privateKeyPem;
        private readonly string _sendUrl;

        private readonly SemaphoreSlim _tokenLock = new(1, 1);
        private string _accessToken = string.Empty;
        private DateTime _accessTokenExpiresUtc = DateTime.MinValue;

        public FcmSender(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;

            string json = ReadServiceAccountJson(configuration);
            string projectId = configuration["FCM:ProjectId"] ?? string.Empty;
            string clientEmail = string.Empty;
            string privateKey = string.Empty;

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var sa = JObject.Parse(json);
                    clientEmail = sa.Value<string>("client_email") ?? string.Empty;
                    privateKey = sa.Value<string>("private_key") ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(projectId)) projectId = sa.Value<string>("project_id") ?? string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.Write(ex);
                }
            }

            _clientEmail = clientEmail;
            _privateKeyPem = privateKey;
            _projectId = projectId;
            _sendUrl = "https://fcm.googleapis.com/v1/projects/" + _projectId + "/messages:send";
        }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(_projectId) &&
            !string.IsNullOrWhiteSpace(_clientEmail) &&
            !string.IsNullOrWhiteSpace(_privateKeyPem);

        private static string ReadServiceAccountJson(IConfiguration configuration)
        {
            string inline = configuration["FCM:ServiceAccountJson"] ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(inline)) return inline;

            string path = configuration["FCM:ServiceAccountFile"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            try { return File.Exists(path) ? File.ReadAllText(path) : string.Empty; }
            catch { return string.Empty; }
        }

        // ------------------------------------------------------------------ send

        public async Task<FcmResult> SendAsync(
            string token, string platform, string title, string body, string kind,
            string? imageUrl, string? actionUrl, int notificationId, CancellationToken ct = default)
        {
            if (!IsConfigured) return FcmResult.NotConfigured;
            if (string.IsNullOrWhiteSpace(token) || token.Length <= 8) return FcmResult.TokenGone;

            try
            {
                string accessToken = await GetAccessTokenAsync(ct);
                if (string.IsNullOrWhiteSpace(accessToken)) return FcmResult.Failed;

                var payload = BuildMessage(token, platform, title, body, kind, imageUrl, actionUrl, notificationId);

                var client = _httpClientFactory.CreateClient();
                using var request = new HttpRequestMessage(HttpMethod.Post, _sendUrl)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"),
                };
                request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

                using var response = await client.SendAsync(request, ct);
                if (response.IsSuccessStatusCode) return FcmResult.Sent;

                string errorBody = await response.Content.ReadAsStringAsync(ct);

                // 404 UNREGISTERED means the device is gone for good. Anything else (429, 5xx, a
                // bad service account) is worth keeping the token for - the next alert may well
                // get through.
                //
                // 400 INVALID_ARGUMENT is only fatal to the TOKEN when Firebase says the token is
                // what it objected to. The same code comes back for a bad payload - an image URL
                // that is not absolute, say - and treating that as a dead device would unregister
                // a phone per recipient and turn push off across the network over one malformed
                // broadcast. A payload fault is logged and the token kept.
                bool tokenRejected =
                    errorBody.Contains("UNREGISTERED", StringComparison.OrdinalIgnoreCase) ||
                    errorBody.Contains("registration-token-not-registered", StringComparison.OrdinalIgnoreCase) ||
                    errorBody.Contains("INVALID_REGISTRATION", StringComparison.OrdinalIgnoreCase) ||
                    (errorBody.Contains("INVALID_ARGUMENT", StringComparison.OrdinalIgnoreCase) &&
                     errorBody.Contains("token", StringComparison.OrdinalIgnoreCase));

                bool dead =
                    response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                    tokenRejected;

                if (!dead)
                    _logger.Write(new Exception("FCM send failed (" + (int)response.StatusCode + "): " + Truncate(errorBody, 500)));

                return dead ? FcmResult.TokenGone : FcmResult.Failed;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return FcmResult.Failed;
            }
        }

        private static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max);

        private static object BuildMessage(
            string token, string platform, string title, string body, string kind,
            string? imageUrl, string? actionUrl, int notificationId)
        {
            // Data travels on every platform and is what the app reads when it is in the
            // foreground (or when the user taps the notification) to open the right screen.
            var data = new Dictionary<string, string>
            {
                ["notificationId"] = notificationId.ToString(),
                ["kind"] = kind,
                ["title"] = title,
                ["body"] = body,
                ["url"] = actionUrl ?? string.Empty,
                ["image"] = imageUrl ?? string.Empty,
            };

            var notification = new Dictionary<string, object> { ["title"] = title, ["body"] = body };
            if (!string.IsNullOrWhiteSpace(imageUrl)) notification["image"] = imageUrl!;

            var message = new Dictionary<string, object>
            {
                ["token"] = token,
                ["notification"] = notification,
                ["data"] = data,
            };

            if (!platform.Equals("web", StringComparison.OrdinalIgnoreCase))
            {
                message["android"] = new Dictionary<string, object>
                {
                    ["priority"] = "HIGH",
                    ["notification"] = new Dictionary<string, object>
                    {
                        // Without a channel that the app has created, Android 8+ drops this.
                        ["channel_id"] = NotificationKind.ChannelFor(kind),
                        ["sound"] = "default",
                        ["default_vibrate_timings"] = true,
                        ["notification_priority"] = "PRIORITY_HIGH",
                    },
                };
            }
            else
            {
                var webpush = new Dictionary<string, object>
                {
                    ["headers"] = new Dictionary<string, string> { ["Urgency"] = "high" },
                };
                if (!string.IsNullOrWhiteSpace(actionUrl))
                    webpush["fcm_options"] = new Dictionary<string, string> { ["link"] = actionUrl! };
                message["webpush"] = webpush;
            }

            return new Dictionary<string, object> { ["message"] = message };
        }

        // ------------------------------------------------------------ oauth2

        /// <summary>
        /// Service-account access token, cached until shortly before it expires. Google issues
        /// these for an hour; re-minting one per push would be a round trip per device.
        /// </summary>
        private async Task<string> GetAccessTokenAsync(CancellationToken ct)
        {
            if (DateTime.UtcNow < _accessTokenExpiresUtc && !string.IsNullOrWhiteSpace(_accessToken))
                return _accessToken;

            await _tokenLock.WaitAsync(ct);
            try
            {
                if (DateTime.UtcNow < _accessTokenExpiresUtc && !string.IsNullOrWhiteSpace(_accessToken))
                    return _accessToken;

                string assertion = BuildSignedJwt();
                var client = _httpClientFactory.CreateClient();
                using var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                    new KeyValuePair<string, string>("assertion", assertion),
                });

                using var response = await client.PostAsync(TokenEndpoint, content, ct);
                string json = await response.Content.ReadAsStringAsync(ct);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.Write(new Exception("FCM token request failed (" + (int)response.StatusCode + "): " + Truncate(json, 500)));
                    return string.Empty;
                }

                var parsed = JObject.Parse(json);
                _accessToken = parsed.Value<string>("access_token") ?? string.Empty;
                int expiresIn = parsed.Value<int?>("expires_in") ?? 3600;
                _accessTokenExpiresUtc = DateTime.UtcNow.AddSeconds(Math.Max(60, expiresIn - 300));
                return _accessToken;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return string.Empty;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private string BuildSignedJwt()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            string header = Base64Url(Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(new { alg = "RS256", typ = "JWT" })));

            string claims = Base64Url(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
            {
                iss = _clientEmail,
                scope = Scope,
                aud = TokenEndpoint,
                iat = now,
                exp = now + 3600,
            })));

            string unsigned = header + "." + claims;

            using var rsa = RSA.Create();
            rsa.ImportFromPem(_privateKeyPem.Replace("\\n", "\n"));
            byte[] signature = rsa.SignData(
                Encoding.UTF8.GetBytes(unsigned), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return unsigned + "." + Base64Url(signature);
        }

        private static string Base64Url(byte[] bytes) =>
            Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
