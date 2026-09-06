using FintechApp.Entities;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The one shared inbound callback pipeline: identify the provider, verify the push, settle it.
    ///
    /// Every upstream gets the same address with its own key —
    /// <c>/api/Callback/p/{CallbackKey}</c> — so adding a provider never means adding an endpoint,
    /// and the security, logging and settlement rules cannot drift apart per provider.
    /// </summary>
    public interface IProviderCallbackRouter
    {
        /// <summary>Handle a push that arrived on the shared address for <paramref name="callbackKey"/>.</summary>
        Task<string> Route(string callbackKey, HttpRequest request, string ipAddress);

        /// <summary>Handle a push for an already-resolved provider — used by the legacy per-provider
        /// endpoints that were published before the shared address existed.</summary>
        Task<string> Route(ProviderMaster provider, HttpRequest request, string ipAddress);
    }
}
