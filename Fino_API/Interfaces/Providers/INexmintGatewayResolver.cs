using Fino.NexmintApi.Configuration;

namespace FintechApp.Interfaces.Providers
{
    // The active Nexmint gateway: the credentials to call with, plus the identity of the linked
    // "Add API" provider so callers can stamp ProviderID/name onto transaction + log rows (otherwise
    // payout logs show up with a blank provider and can't be filtered by provider in the Provider Log).
    public class NexmintGatewayInfo
    {
        public NexmintCredentials Credentials { get; set; } = new();
        public int ProviderID { get; set; }                    // ProviderMaster id
        public string ProviderName { get; set; } = string.Empty;

        // Sequence of the gateway row this came from. Two coded rails now read the same list, so the
        // dispatcher needs the number to answer "which active row is actually in force" rather than
        // whichever resolver it happened to ask first.
        public int Sequence { get; set; }
    }

    // Resolves the currently-active Nexmint payout gateway: the active "DMT Settings" row (lowest
    // Sequence first) that links — via its ProviderCode = ProviderName — to an active Nexmint provider
    // registered in "Add API". Returns null when no Nexmint gateway is active, so the caller can fall
    // back safely.
    public interface INexmintGatewayResolver
    {
        Task<NexmintGatewayInfo?> ResolveActiveGateway();
    }
}
