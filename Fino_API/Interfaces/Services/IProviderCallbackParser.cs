using FintechApp.Entities;
using FintechApp.StaticModels;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Turns ONE provider's push into the host's neutral <see cref="ProviderCallbackParseResult"/>.
    ///
    /// This is the only per-provider piece of the shared callback address. The controller resolves
    /// the provider from the URL key and the router picks the first parser that claims it; nothing
    /// else in the host learns a provider's field names, status words or signature scheme.
    ///
    /// Registration order in Program.cs IS the match order — specific parsers first, the generic
    /// one last, since it claims everything.
    /// </summary>
    public interface IProviderCallbackParser
    {
        /// <summary>True when this parser understands the given provider's push format.</summary>
        bool CanHandle(ProviderMaster provider);

        /// <param name="values">Everything the request carried — query string, form fields and a
        /// parsed JSON body merged into one case-insensitive bag.</param>
        /// <param name="rawBody">The body exactly as received, for the audit log.</param>
        /// <param name="headers">The request headers, for providers that prove a push with a signed
        /// header rather than a field in the body. Separate from <paramref name="values"/> so a body
        /// field can never pose as one.</param>
        ProviderCallbackParseResult Parse(ProviderMaster provider, IDictionary<string, string> values, string rawBody,
            IDictionary<string, string> headers);
    }
}
