using System.Security.Cryptography;
using System.Text;

namespace FintechApp.Helpers
{
    /// <summary>
    /// Time-based one-time passwords (RFC 6238) — the codes Google Authenticator, Microsoft
    /// Authenticator, Authy and 2FAS generate.
    ///
    /// Written out rather than taken from a package: the algorithm is thirty lines of HMAC and
    /// the only hard part is agreeing on the defaults every authenticator app assumes — SHA-1,
    /// 6 digits, a 30-second step counted from the Unix epoch. Those are not configurable here
    /// on purpose; Google Authenticator ignores the algorithm/digits parameters in a provisioning
    /// URI, so a server that used anything else would hand out QR codes that silently produce
    /// codes the app shows but the server rejects.
    ///
    /// Costs nothing to run and nothing to send — which is the point next to SMS OTP.
    /// </summary>
    public static class Totp
    {
        private const int StepSeconds = 30;
        private const int Digits = 6;

        /// <summary>How many steps either side of "now" are accepted (±1 ⇒ a 90-second window).</summary>
        private const int DefaultWindow = 1;

        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        /// <summary>
        /// A fresh shared secret, Base32-encoded. 20 bytes (160 bits) is what RFC 4226 specifies
        /// for HMAC-SHA1 and what every authenticator app expects to be handed.
        /// </summary>
        public static string GenerateSecret()
        {
            var bytes = RandomNumberGenerator.GetBytes(20);
            return ToBase32(bytes);
        }

        /// <summary>
        /// The step number for a moment in time. Stored after a successful verification so the
        /// same code cannot be replayed inside its own validity window — without it, a code read
        /// over someone's shoulder stays usable for up to another 90 seconds.
        /// </summary>
        public static long CurrentStep(DateTime utcNow) =>
            (long)(utcNow - DateTime.UnixEpoch).TotalSeconds / StepSeconds;

        /// <summary>
        /// Checks <paramref name="code"/> against <paramref name="secretBase32"/>, allowing for
        /// clock drift between the server and the phone.
        ///
        /// Returns the step the code matched, or null when it matches none. Callers persist that
        /// step and refuse anything at or below it next time.
        /// </summary>
        public static long? Verify(string secretBase32, string code, DateTime utcNow, int window = DefaultWindow)
        {
            if (string.IsNullOrWhiteSpace(secretBase32) || string.IsNullOrWhiteSpace(code)) return null;

            // Authenticator apps display the code in two groups ("123 456"); people paste it that way.
            code = new string(code.Where(char.IsDigit).ToArray());
            if (code.Length != Digits) return null;

            byte[] key;
            try { key = FromBase32(secretBase32); }
            catch { return null; }
            if (key.Length == 0) return null;

            long current = CurrentStep(utcNow);
            for (long step = current - window; step <= current + window; step++)
            {
                if (FixedTimeEquals(Compute(key, step), code)) return step;
            }
            return null;
        }

        /// <summary>
        /// The <c>otpauth://</c> URI that goes into the QR code.
        ///
        /// The label carries the issuer twice — once as the "Issuer:Account" prefix and once as the
        /// query parameter — because different apps read different halves, and an account that
        /// shows up in the app as a bare mobile number with no brand beside it is the reason people
        /// delete the wrong entry.
        /// </summary>
        public static string BuildProvisioningUri(string issuer, string account, string secretBase32)
        {
            string i = Uri.EscapeDataString(string.IsNullOrWhiteSpace(issuer) ? "Fino" : issuer.Trim());
            string a = Uri.EscapeDataString(string.IsNullOrWhiteSpace(account) ? "account" : account.Trim());
            return $"otpauth://totp/{i}:{a}?secret={secretBase32}&issuer={i}&algorithm=SHA1&digits={Digits}&period={StepSeconds}";
        }

        /// <summary>The secret in groups of four, which is how it is typed in by hand.</summary>
        public static string FormatSecretForDisplay(string secretBase32)
        {
            if (string.IsNullOrWhiteSpace(secretBase32)) return string.Empty;
            return string.Join(' ', Enumerable.Range(0, (secretBase32.Length + 3) / 4)
                .Select(i => secretBase32.Substring(i * 4, Math.Min(4, secretBase32.Length - i * 4))));
        }

        // ─── RFC 4226 ────────────────────────────────────────────────────────

        private static string Compute(byte[] key, long step)
        {
            byte[] counter = BitConverter.GetBytes(step);
            if (BitConverter.IsLittleEndian) Array.Reverse(counter);

            using var hmac = new HMACSHA1(key);
            byte[] hash = hmac.ComputeHash(counter);

            int offset = hash[^1] & 0x0F;
            int binary = ((hash[offset] & 0x7F) << 24)
                       | ((hash[offset + 1] & 0xFF) << 16)
                       | ((hash[offset + 2] & 0xFF) << 8)
                       | (hash[offset + 3] & 0xFF);

            return (binary % (int)Math.Pow(10, Digits)).ToString(new string('0', Digits));
        }

        /// <summary>Length-independent comparison, so a wrong code cannot be narrowed down by timing.</summary>
        private static bool FixedTimeEquals(string a, string b) =>
            CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(a), Encoding.ASCII.GetBytes(b));

        // ─── Base32 (RFC 4648, no padding — what authenticator apps use) ─────

        private static string ToBase32(byte[] data)
        {
            var sb = new StringBuilder((data.Length * 8 + 4) / 5);
            int buffer = 0, bitsLeft = 0;
            foreach (byte b in data)
            {
                buffer = (buffer << 8) | b;
                bitsLeft += 8;
                while (bitsLeft >= 5)
                {
                    sb.Append(Base32Alphabet[(buffer >> (bitsLeft - 5)) & 31]);
                    bitsLeft -= 5;
                }
            }
            if (bitsLeft > 0) sb.Append(Base32Alphabet[(buffer << (5 - bitsLeft)) & 31]);
            return sb.ToString();
        }

        private static byte[] FromBase32(string input)
        {
            // Tolerate what a person types back in: spaces, lower case and '=' padding.
            input = (input ?? string.Empty).Replace(" ", string.Empty).Replace("=", string.Empty).ToUpperInvariant();

            var bytes = new List<byte>(input.Length * 5 / 8);
            int buffer = 0, bitsLeft = 0;
            foreach (char c in input)
            {
                int value = Base32Alphabet.IndexOf(c);
                if (value < 0) throw new FormatException("Not a Base32 secret.");
                buffer = (buffer << 5) | value;
                bitsLeft += 5;
                if (bitsLeft >= 8)
                {
                    bytes.Add((byte)(buffer >> (bitsLeft - 8)));
                    bitsLeft -= 8;
                }
            }
            return bytes.ToArray();
        }
    }
}
