using FintechApp.Entities;

namespace FintechApp.Interfaces.Providers
{
    /// <summary>
    /// Finds the compiled integration behind a provider row, and answers the two questions the
    /// admin screens ask about one.
    ///
    /// Everything that wanted to know "is this provider coded, and what can it serve?" used to
    /// name Pay1 and JaiJui itself. This is the single answer, so a new coded provider becomes
    /// visible to the routing grid, the provider list and the gateway at the same moment — by
    /// being registered, not by being mentioned in three more places.
    /// </summary>
    public interface ICodedProviderRegistry
    {
        /// <summary>The adapter for this provider, or null when it is an ordinary template gateway.</summary>
        ICodedProvider? For(ProviderMaster provider);

        /// <summary>True when a provider of this name is a compiled integration.</summary>
        bool IsCoded(string? providerName);

        /// <summary>
        /// The rail a coded provider serves (BBPS / DMT / …), or empty for a template gateway.
        /// Matched on the name alone, because the callers that need it — the provider list and
        /// the Provider Routing pool — have a name and not always a full row.
        /// </summary>
        string FamilyOf(string? providerName);
    }
}
