using Fino.JaiJuiApi.Configuration;
using FintechApp.Entities;

namespace FintechApp.Providers
{
    // Single source of truth for recognising a JaiJui gateway and turning it into the Fino.JaiJuiApi
    // per-call credential override. Credentials live on the ProviderMaster ("Add API") row — NOT in
    // appsettings — so an admin can add / edit / activate JaiJui entirely from the UI:
    //
    //   ProviderMaster.LoginID          -> Mobile   (KeyData header)
    //   ProviderMaster.Token            -> ApiToken (KeyData header)
    //   ProviderMaster.ProviderUniqueID -> AuthCode (request body)
    //   ProviderMaster.ProviderUrl      -> BaseUrl  (blank -> the "JaiJuiApi" config default URL)
    //
    // A "DMT Settings" row links to one of these providers by putting the provider's name in its
    // ProviderCode field; the DMT pipeline then resolves the credentials from the linked provider.
    public static class JaiJuiProviderResolver
    {
        // Normalised marker: matches "JaiJui", "jaijui", "JAI JUI", "JaiJui DMT", …
        public const string Marker = "JAIJUI";

        public static bool IsJaiJui(string? value) =>
            !string.IsNullOrWhiteSpace(value) &&
            value.Replace(" ", string.Empty).Contains(Marker, StringComparison.OrdinalIgnoreCase);

        public static JaiJuiCredentials CredentialsFrom(ProviderMaster provider) => new()
        {
            BaseUrl = provider.ProviderUrl?.Trim() ?? string.Empty,      // blank -> config default
            Mobile = provider.LoginID?.Trim() ?? string.Empty,
            ApiToken = provider.Token?.Trim() ?? string.Empty,
            AuthCode = provider.ProviderUniqueID?.Trim() ?? string.Empty
        };
    }
}
