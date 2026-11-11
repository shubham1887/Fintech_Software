using Fino.NexmintApi.Configuration;
using FintechApp.Entities;

namespace FintechApp.Providers
{
    // Single source of truth for recognising a Nexmint gateway and turning it into the Fino.NexmintApi
    // per-call credential override. Credentials live on the ProviderMaster ("Add API") row — NOT in
    // appsettings — so an admin can add / edit / activate Nexmint entirely from the UI:
    //
    //   ProviderMaster.LoginID          -> ClientID      (client-id header)
    //   ProviderMaster.Token            -> ClientSecret  (signs both JWTs, HS256)
    //   ProviderMaster.ProviderUniqueID -> WebhookSecret (signs the payout callback, a DIFFERENT key)
    //   ProviderMaster.ProviderUrl      -> BaseUrl       (blank -> the "NexmintApi" config default URL;
    //                                                     a URL ending in /sandbox switches to test money)
    //
    // A "DMT Settings" row links to one of these providers by putting the provider's name in its
    // ProviderCode field; the payout pipeline then resolves the credentials from the linked provider.
    public static class NexmintProviderResolver
    {
        // Normalised marker: matches "Nexmint", "nexmint", "NEX MINT", "Nexmint Payout", …
        public const string Marker = "NEXMINT";

        public static bool IsNexmint(string? value) =>
            !string.IsNullOrWhiteSpace(value) &&
            value.Replace(" ", string.Empty).Contains(Marker, StringComparison.OrdinalIgnoreCase);

        public static NexmintCredentials CredentialsFrom(ProviderMaster provider) => new()
        {
            BaseUrl = provider.ProviderUrl?.Trim() ?? string.Empty,      // blank -> config default
            ClientID = provider.LoginID?.Trim() ?? string.Empty,
            ClientSecret = provider.Token?.Trim() ?? string.Empty,
            WebhookSecret = provider.ProviderUniqueID?.Trim() ?? string.Empty
            // UseSandbox stays null on purpose: the provider row says nothing about it, so the
            // "NexmintApi" section decides, and a blank row cannot silently send live money on a
            // test install (or test money on a live one).
        };
    }
}
