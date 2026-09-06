using System.Security.Cryptography;

namespace FintechApp.Providers
{
    // The unique id every provider integration gets, and the one address shape we hand out with it.
    //
    // One callback URL for every upstream — Payin, Pay1, JaiJui, whatever is added next:
    //
    //     https://<our-host>/api/Callback/p/pv_4f8c1a09e73b52d6
    //                                        ^^^^^^^^^^^^^^^^^^ ProviderMaster.CallbackKey
    //
    // The key is what tells us WHO is pushing, so the host never has to guess the format from the
    // body or add a new endpoint per provider. It is deliberately NOT a credential: it travels in a
    // URL, it is visible in the provider's panel and in our logs, so it must be safe to leak.
    // Authenticity is a separate concern, proved per provider (Pay1's hash_code, an IP allowlist).
    public static class ProviderCallbackKey
    {
        // A visible prefix so an operator can tell at a glance what a bare string in a provider's
        // dashboard is, and 12 random bytes so the key cannot be enumerated.
        public const string Prefix = "pv_";

        public static string New() =>
            Prefix + Convert.ToHexString(RandomNumberGenerator.GetBytes(12)).ToLowerInvariant();

        /// <summary>Keys compare case-insensitively — providers echo URLs back in odd casings.</summary>
        public static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();

        /// <summary>Path we publish for a key. Kept here so the shape lives in exactly one place.</summary>
        public static string PathFor(string? key) => $"api/Callback/p/{Normalize(key)}";
    }
}
