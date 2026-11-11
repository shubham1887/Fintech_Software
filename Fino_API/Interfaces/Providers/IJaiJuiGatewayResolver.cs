using Fino.JaiJuiApi.Configuration;

namespace FintechApp.Interfaces.Providers
{
    // The active JaiJui gateway: the credentials to call with, plus the identity of the linked
    // "Add API" provider so callers can stamp ProviderID/name onto transaction + log rows (otherwise
    // DMT logs show up with a blank provider and can't be filtered by provider in the Provider Log).
    public class JaiJuiGatewayInfo
    {
        public JaiJuiCredentials Credentials { get; set; } = new();
        public int ProviderID { get; set; }                    // ProviderMaster id (0 = legacy DMT-row creds)
        public string ProviderName { get; set; } = string.Empty;

        // Sequence of the gateway row this came from. Two coded rails now read the same list, so the
        // dispatcher needs the number to answer "which active row is actually in force" rather than
        // whichever resolver it happened to ask first.
        public int Sequence { get; set; }
    }

    // Resolves the currently-active JaiJui DMT gateway: the active "DMT Settings" row (lowest Sequence
    // first) that links — via its ProviderCode = ProviderName — to an active JaiJui provider registered
    // in "Add API". Returns null when no JaiJui gateway is active, so the caller can fall back safely.
    public interface IJaiJuiGatewayResolver
    {
        Task<JaiJuiGatewayInfo?> ResolveActiveGateway();
    }
}
