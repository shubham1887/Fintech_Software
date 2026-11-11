using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;
using Microsoft.Extensions.Caching.Memory;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IApiUserRequestSecurityService"/>
    public class ApiUserRequestSecurityService : IApiUserRequestSecurityService
    {
        /// <summary>Cache key prefix for spent nonces. Scoped per user so two partners cannot collide.</summary>
        private const string NoncePrefix = "apiuser:nonce:";

        /// <summary>A nonce longer than this is refused outright rather than cached.</summary>
        private const int MaxNonceLength = 128;

        /// <summary>…and shorter than this has too little entropy to be a meaningful one-shot value.</summary>
        private const int MinNonceLength = 8;

        private readonly IMemoryCache _cache;
        private readonly Logger _logger = new();

        private readonly int _clockSkewSeconds;
        private readonly int _nonceTtlSeconds;

        public bool SignatureRequired { get; }

        public ApiUserRequestSecurityService(IMemoryCache cache, IConfiguration configuration)
        {
            _cache = cache;

            var cfg = configuration.GetSection("ApiUser:Security");

            // Signing is ON unless a deployment explicitly turns it off. A money-moving endpoint
            // that ships insecure-by-default is a configuration accident waiting to happen, so the
            // safe value is the one you get by doing nothing.
            SignatureRequired = cfg.GetValue("RequireSignature", true);

            // ±5 minutes absorbs ordinary NTP drift on a partner's server without leaving a
            // captured request usefully replayable.
            _clockSkewSeconds = Math.Clamp(cfg.GetValue("ClockSkewSeconds", 300), 30, 3600);

            // A nonce must stay remembered for at least the whole window in which its timestamp is
            // still acceptable, or a replay could arrive after we forgot it. Both edges of the skew
            // window count, hence 2× — plus a margin, and never less than the skew itself.
            _nonceTtlSeconds = Math.Max(
                cfg.GetValue("NonceTtlSeconds", 0) > 0 ? cfg.GetValue("NonceTtlSeconds", 0) : _clockSkewSeconds * 2 + 60,
                _clockSkewSeconds);
        }

        public ApiUserSecurityResult Verify(ApiUserSignedRequest request, int userId, string apiToken)
        {
            try
            {
                bool supplied = !string.IsNullOrWhiteSpace(request.Signature);

                // Not signed and not required: let it through on the token alone. Still refused
                // outright when required — this is the only path that can skip the crypto.
                if (!supplied)
                {
                    return SignatureRequired
                        ? ApiUserSecurityResult.Fail(
                            ApiUserErrorCode.SignatureRequired,
                            "Request signature is required. Send X-Api-Timestamp, X-Api-Nonce and X-Api-Signature.")
                        : ApiUserSecurityResult.Ok();
                }

                // From here the caller claims to be signing, so every part of the scheme is
                // mandatory regardless of the switch — a half-signed request is never acceptable.
                var timestampCheck = CheckTimestamp(request.Timestamp);
                if (!timestampCheck.Succeeded) return timestampCheck;

                var nonceCheck = CheckNonce(request.Nonce, userId);
                if (!nonceCheck.Succeeded) return nonceCheck;

                string canonical = ApiUserSignature.Canonical(
                    request.Method, request.Path, request.Timestamp, request.Nonce, request.Body);

                if (!ApiUserSignature.Verify(request.Signature, ApiUserSignature.Compute(canonical, apiToken)))
                {
                    // Deliberately uninformative: revealing WHICH component of the canonical string
                    // disagreed would turn the error into an oracle. The documentation carries a
                    // worked example instead.
                    return ApiUserSecurityResult.Fail(
                        ApiUserErrorCode.SignatureInvalid,
                        "Signature verification failed. Check the canonical string, the raw body bytes and the token used as the key.");
                }

                // Burn the nonce only once the request is fully proven. Spending it earlier would
                // let an attacker invalidate a legitimate caller's nonce with an unsigned guess.
                SpendNonce(request.Nonce, userId);

                return ApiUserSecurityResult.Ok();
            }
            catch (Exception ex)
            {
                // A fault inside the gate must fail CLOSED. The alternative — letting a request
                // through because the verifier itself broke — is the one outcome worth avoiding
                // more than an outage on this endpoint.
                _logger.Write(ex);
                return ApiUserSecurityResult.Fail(
                    ApiUserErrorCode.SignatureInvalid, "Signature could not be verified.");
            }
        }

        /// <summary>
        /// The timestamp must parse as unix epoch seconds and sit inside the skew window. Future
        /// timestamps are bounded by the same window: a caller whose clock runs fast is tolerated,
        /// a request minted far in the future to survive the nonce TTL is not.
        /// </summary>
        private ApiUserSecurityResult CheckTimestamp(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return ApiUserSecurityResult.Fail(
                    ApiUserErrorCode.TimestampInvalid, "Parameter 'timestamp' is required (unix epoch seconds, UTC).");

            if (!long.TryParse(raw.Trim(), out long epoch))
                return ApiUserSecurityResult.Fail(
                    ApiUserErrorCode.TimestampInvalid, "Parameter 'timestamp' must be unix epoch SECONDS, UTC.");

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long drift = Math.Abs(now - epoch);

            if (drift > _clockSkewSeconds)
                return ApiUserSecurityResult.Fail(
                    ApiUserErrorCode.TimestampInvalid,
                    $"Timestamp is {drift}s away from server time; the accepted window is ±{_clockSkewSeconds}s. Synchronise your clock (NTP).");

            return ApiUserSecurityResult.Ok();
        }

        /// <summary>
        /// The nonce must be present, of sane length, and unused inside the replay window.
        /// </summary>
        private ApiUserSecurityResult CheckNonce(string raw, int userId)
        {
            string nonce = (raw ?? string.Empty).Trim();

            if (nonce.Length == 0)
                return ApiUserSecurityResult.Fail(
                    ApiUserErrorCode.NonceInvalid, "Parameter 'nonce' is required and must be unique per request.");

            if (nonce.Length < MinNonceLength || nonce.Length > MaxNonceLength)
                return ApiUserSecurityResult.Fail(
                    ApiUserErrorCode.NonceInvalid,
                    $"Parameter 'nonce' must be {MinNonceLength}-{MaxNonceLength} characters. A UUID is a good choice.");

            if (_cache.TryGetValue(NonceKey(nonce, userId), out _))
                return ApiUserSecurityResult.Fail(
                    ApiUserErrorCode.NonceInvalid,
                    "This nonce has already been used. Generate a fresh nonce for every request.");

            return ApiUserSecurityResult.Ok();
        }

        /// <summary>Marks a nonce spent for the whole replay window.</summary>
        private void SpendNonce(string nonce, int userId) =>
            _cache.Set(NonceKey(nonce.Trim(), userId), true, TimeSpan.FromSeconds(_nonceTtlSeconds));

        private static string NonceKey(string nonce, int userId) => $"{NoncePrefix}{userId}:{nonce}";
    }
}
