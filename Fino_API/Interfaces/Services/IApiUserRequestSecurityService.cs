namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The verdict of the request-security gate. Mirrors the shape of
    /// <c>ApiUserAuthResult</c> so the gateway can handle a signature failure and a credential
    /// failure through the same branch.
    /// </summary>
    public class ApiUserSecurityResult
    {
        public bool Succeeded { get; private set; }
        public string ErrorCode { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;

        public static ApiUserSecurityResult Ok() => new() { Succeeded = true };

        public static ApiUserSecurityResult Fail(string errorCode, string message) =>
            new() { Succeeded = false, ErrorCode = errorCode, Message = message };
    }

    /// <summary>
    /// Everything the caller must present on a signed request, lifted out of the HTTP layer so
    /// the verifier never touches <c>HttpContext</c> and stays unit-testable.
    /// </summary>
    public class ApiUserSignedRequest
    {
        /// <summary>HTTP method as received.</summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>Request path without the query string.</summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>Unix epoch SECONDS, exactly the characters the caller sent.</summary>
        public string Timestamp { get; set; } = string.Empty;

        /// <summary>The caller's single-use value for this request.</summary>
        public string Nonce { get; set; } = string.Empty;

        /// <summary>Lowercase hex HMAC-SHA256 supplied by the caller.</summary>
        public string Signature { get; set; } = string.Empty;

        /// <summary>Raw body bytes as received; empty for a GET.</summary>
        public byte[] Body { get; set; } = Array.Empty<byte>();
    }

    /// <summary>
    /// Verifies the HMAC request signature, the timestamp freshness and the nonce single-use rule
    /// for the money-moving B2B endpoints.
    ///
    /// Kept separate from <see cref="IApiUserAuthService"/> on purpose: authentication answers
    /// "who is this and may they call us", this answers "is this exact request genuine, fresh and
    /// not a replay". Both must pass, and the signature check runs only AFTER authentication has
    /// resolved the user, because the token that keys the HMAC is the user's own.
    /// </summary>
    public interface IApiUserRequestSecurityService
    {
        /// <summary>
        /// True when signing is enforced for this deployment. When false the gate still VERIFIES a
        /// signature the caller chose to send (so a correctly-integrated partner is never silently
        /// downgraded), but does not require one — the switch exists only to let an existing
        /// partner finish their integration, never to run unsigned in steady state.
        /// </summary>
        bool SignatureRequired { get; }

        /// <summary>
        /// Runs the full gate: timestamp skew → nonce single-use → signature match. Returns the
        /// exact contract error code to hand back, never throws.
        /// </summary>
        /// <param name="request">The request's signed material.</param>
        /// <param name="userId">The authenticated API user, used to scope the nonce.</param>
        /// <param name="apiToken">That user's API token — the HMAC key.</param>
        ApiUserSecurityResult Verify(ApiUserSignedRequest request, int userId, string apiToken);
    }
}
