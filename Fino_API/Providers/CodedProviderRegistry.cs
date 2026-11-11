using FintechApp.Entities;
using FintechApp.Interfaces.Providers;

namespace FintechApp.Providers
{
    /// <summary>
    /// The list of compiled integrations, assembled by DI from every registered
    /// <see cref="ICodedProvider"/>. Registration order in Program.cs is the match order.
    ///
    /// This replaced a static class that named Pay1 and JaiJui in code. The difference matters
    /// for a product that is resold: each client arrives with a different aggregator, and
    /// "register one more class" is a change a developer can make without reading the gateway.
    /// </summary>
    public class CodedProviderRegistry : ICodedProviderRegistry
    {
        private readonly IReadOnlyList<ICodedProvider> _providers;

        public CodedProviderRegistry(IEnumerable<ICodedProvider> providers) => _providers = providers.ToList();

        public ICodedProvider? For(ProviderMaster provider) =>
            _providers.FirstOrDefault(p => p.CanHandle(provider));

        public bool IsCoded(string? providerName) => FamilyOf(providerName).Length > 0;

        /// <summary>
        /// Matched on the name alone, because the two callers that need this — the provider list
        /// and the Provider Routing pool — sometimes hold only a name. An adapter's CanHandle is
        /// therefore expected to decide from <see cref="ProviderMaster.ProviderName"/>; the probe
        /// row below is what it is handed here.
        /// </summary>
        public string FamilyOf(string? providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName)) return string.Empty;
            var probe = new ProviderMaster { ProviderName = providerName };
            return _providers.FirstOrDefault(p => p.CanHandle(probe))?.Family ?? string.Empty;
        }
    }
}
