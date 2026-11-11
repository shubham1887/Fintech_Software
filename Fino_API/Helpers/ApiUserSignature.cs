using System.Security.Cryptography;
using System.Text;

namespace FintechApp.Helpers
{
    /// <summary>
    /// The request-signing scheme for the money-moving B2B endpoints (/api/v1/dmt/*).
    ///
    /// The API token alone is a bearer credential: anything that can read one request can replay
    /// it. DMT tickets are large, so the DMT endpoints additionally require the caller to PROVE
    /// possession of the token on every call without transmitting it as the only secret — an
    /// HMAC over the exact bytes being sent, bound to a timestamp and a one-shot nonce.
    ///
    /// That buys three things a bare token cannot:
    ///   • Tamper-evidence — flip one rupee of the body and the signature no longer matches.
    ///   • Replay resistance — the timestamp ages out and the nonce is single-use.
    ///   • Endpoint binding — the method and path are inside the signed string, so a captured
    ///     /beneficiary/list call cannot be re-pointed at /transfer.
    ///
    /// The canonical string is deliberately newline-joined and fixed-order rather than a sorted
    /// parameter map: the body is hashed as RAW BYTES, so the integrator never has to reproduce
    /// our JSON serialisation, key order or number formatting to sign correctly. Whatever bytes
    /// they put on the wire are the bytes they hash.
    ///
    /// <code>
    ///   canonical = METHOD  "\n"
    ///               PATH    "\n"     (lowercase, no query string)
    ///               TIMESTAMP "\n"   (unix epoch SECONDS, exactly as sent)
    ///               NONCE   "\n"
    ///               SHA256(raw body bytes) as lowercase hex
    ///
    ///   signature = HMAC-SHA256(canonical, api_token) as lowercase hex
    /// </code>
    ///
    /// This mirrors <c>ApiUserCallbackSender</c>, which signs OUTBOUND callbacks with the same
    /// token and the same algorithm — one shared secret, one algorithm, both directions.
    /// </summary>
    public static class ApiUserSignature
    {
        /// <summary>Published algorithm name, echoed in headers and documentation.</summary>
        public const string Algorithm = "HMAC-SHA256";

        /// <summary>
        /// Builds the exact string that gets signed. Public so the documentation, the tests and
        /// the verifier can never drift from one another.
        /// </summary>
        /// <param name="method">HTTP method; case-insensitive, upper-cased here.</param>
        /// <param name="path">Request path without query string, e.g. /api/v1/dmt/transfer.</param>
        /// <param name="timestamp">Unix epoch seconds, exactly the characters the caller sent.</param>
        /// <param name="nonce">The caller's single-use value, exactly as sent.</param>
        /// <param name="body">Raw request body bytes; empty for a GET.</param>
        public static string Canonical(string method, string path, string timestamp, string nonce, byte[]? body)
        {
            var sb = new StringBuilder();
            sb.Append((method ?? string.Empty).Trim().ToUpperInvariant()).Append('\n');
            sb.Append((path ?? string.Empty).Trim().ToLowerInvariant()).Append('\n');
            sb.Append((timestamp ?? string.Empty).Trim()).Append('\n');
            sb.Append((nonce ?? string.Empty).Trim()).Append('\n');
            sb.Append(Sha256Hex(body ?? Array.Empty<byte>()));
            return sb.ToString();
        }

        /// <summary>Lowercase hex SHA-256 of the given bytes.</summary>
        public static string Sha256Hex(byte[] data) =>
            Convert.ToHexString(SHA256.HashData(data ?? Array.Empty<byte>())).ToLowerInvariant();

        /// <summary>Lowercase hex HMAC-SHA256 of <paramref name="canonical"/> keyed by the API token.</summary>
        public static string Compute(string canonical, string apiToken)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiToken ?? string.Empty));
            return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical ?? string.Empty)))
                .ToLowerInvariant();
        }

        /// <summary>
        /// Compares a supplied signature against the expected one in constant time.
        ///
        /// A plain string comparison returns as soon as two characters differ, and that timing
        /// difference is enough to recover a valid signature byte by byte. Both values are
        /// normalised to lowercase hex bytes first so that casing or padding never decides the
        /// result, and the comparison itself is fixed-time over the full length.
        /// </summary>
        public static bool Verify(string? supplied, string expected)
        {
            string a = (supplied ?? string.Empty).Trim().ToLowerInvariant();
            string b = (expected ?? string.Empty).Trim().ToLowerInvariant();

            // Length is not a secret (it is fixed by the algorithm), so an early return here
            // leaks nothing an attacker does not already know from the published scheme.
            if (a.Length != b.Length || a.Length == 0) return false;

            return CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(a), Encoding.ASCII.GetBytes(b));
        }
    }
}
