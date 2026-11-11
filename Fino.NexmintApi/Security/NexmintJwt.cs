using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Fino.NexmintApi.Security
{
    /// <summary>
    /// The HS256 tokens Nexmint authenticates with, written out by hand rather than taken from a
    /// JWT package.
    ///
    /// Two reasons. The library is a FrameworkReference-only project so it can be dropped into any
    /// host without dragging a dependency graph behind it; and Nexmint rejects a token whose
    /// <c>iat</c> is more than five seconds old, which means the claim is not decoration — it has
    /// to be stamped at the moment of the call, on every call. A helper that quietly reuses a
    /// cached token would turn every request into "Token too old. Must be within 5 seconds."
    /// </summary>
    public static class NexmintJwt
    {
        private const string HeaderJson = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            // The provider reads exactly the field names it documents; nothing may be renamed, and a
            // null must not be sent as a claim the server then fails to validate.
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Sign a payload, stamping <c>iat</c> with the current time. Call it immediately before the
        /// request — the token is good for five seconds against the PROVIDER's clock, so a server
        /// whose own clock has drifted will be refused even though the code is right.
        /// </summary>
        public static string Sign(IDictionary<string, object?> claims, string secret)
        {
            var payload = new Dictionary<string, object?>(claims)
            {
                ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            string head = Base64Url(Encoding.UTF8.GetBytes(HeaderJson));
            string body = Base64Url(JsonSerializer.SerializeToUtf8Bytes(payload, SerializerOptions));
            string signingInput = $"{head}.{body}";

            using var mac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            string signature = Base64Url(mac.ComputeHash(Encoding.UTF8.GetBytes(signingInput)));

            return $"{signingInput}.{signature}";
        }

        /// <summary>The auth token every request carries: a JWT whose only claim is the client id.</summary>
        public static string AuthToken(string clientID, string secret) =>
            Sign(new Dictionary<string, object?> { ["client_id"] = clientID }, secret);

        /// <summary>
        /// Verify a token Nexmint signed (the webhook's Authorization header) and hand back its
        /// payload JSON. Signature only: the age rule is theirs to enforce on us, not ours on a
        /// callback that may legitimately be retried minutes later.
        /// </summary>
        public static bool TryVerify(string token, string secret, out string payloadJson)
        {
            payloadJson = string.Empty;
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(secret)) return false;

            string[] parts = token.Trim().Split('.');
            if (parts.Length != 3) return false;

            try
            {
                using var mac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
                byte[] expected = mac.ComputeHash(Encoding.UTF8.GetBytes($"{parts[0]}.{parts[1]}"));
                byte[] actual = FromBase64Url(parts[2]);

                // Constant-time: a signature check that short-circuits on the first wrong byte tells
                // an attacker how much of a forgery was right.
                if (!CryptographicOperations.FixedTimeEquals(expected, actual)) return false;

                payloadJson = Encoding.UTF8.GetString(FromBase64Url(parts[1]));
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string Base64Url(byte[] bytes) =>
            Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static byte[] FromBase64Url(string value)
        {
            string padded = value.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
        }
    }
}
