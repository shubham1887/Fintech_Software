using Fino.Pay1Api.Configuration;
using FintechApp.Entities;

namespace FintechApp.Providers
{
    // Single source of truth for recognising a Pay1 Utility (BBPS) gateway and turning it into the
    // Fino.Pay1Api per-call credential override. Credentials live on the ProviderMaster
    // ("API Providers") row — NOT in appsettings — so an admin can add / edit / activate Pay1
    // entirely from the UI, exactly as JaiJui is handled:
    //
    //   ProviderMaster.LoginID          -> PartnerId (Pay1 "partner_id" / account id, e.g. P0232050)
    //   ProviderMaster.Token            -> ApiKey    (hashed into hash_code, never transmitted)
    //   ProviderMaster.ProviderUrl      -> BaseUrl   (blank -> the "Pay1Api" config default URL)
    //   ProviderMaster.ProviderUniqueID -> partner id shown in the Pay1 panel (informational)
    //
    // A biller reaches Pay1 the same way any operator reaches any provider: an active
    // ProviderConfiguration route (its OpCode carrying the Pay1 product id) plus an OperatorSequence
    // entry. Nothing about routing, switching or denominations changes for this provider.
    public static class Pay1ProviderResolver
    {
        // Normalised marker: matches "Pay1", "PAY 1", "Pay1 BBPS", "Pay1 Utility", …
        public const string Marker = "PAY1";

        public static bool IsPay1(string? value) =>
            !string.IsNullOrWhiteSpace(value) &&
            value.Replace(" ", string.Empty).Contains(Marker, StringComparison.OrdinalIgnoreCase);

        public static Pay1Credentials CredentialsFrom(ProviderMaster provider) => new()
        {
            BaseUrl = provider.ProviderUrl?.Trim() ?? string.Empty,   // blank -> config default
            PartnerId = provider.LoginID?.Trim() ?? string.Empty,
            ApiKey = provider.Token?.Trim() ?? string.Empty
        };
    }
}
