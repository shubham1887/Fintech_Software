using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    /// <summary>
    /// Read access to every tax master, as whole cached lists.
    ///
    /// One repository rather than six, against the usual one-per-entity habit, because these
    /// masters are not used one at a time: resolving a single transaction needs the rules, the
    /// rate the winning rule points at, its SAC code, possibly a TDS section and the party's
    /// relationship — all as of one date. Six repositories would mean six caches that can expire
    /// at different moments, and a rule read with a rate from a different generation is exactly
    /// the kind of inconsistency this subsystem exists to stop.
    ///
    /// Writes go through the master services, which call <see cref="InvalidateCache"/>. A stale
    /// tax rule stamps the wrong rate on real money, so there is no sliding expiry here: the
    /// cache is dropped on write.
    /// </summary>
    public interface ITaxConfigurationRepository
    {
        Task<List<TaxRateMaster>> GetRates();

        Task<List<HSNSACMaster>> GetHSNSAC();

        Task<List<TDSSectionMaster>> GetTDSSections();

        Task<List<TaxRuleMaster>> GetRules();

        Task<List<PartyServiceRelationship>> GetPartyServiceRelationships();

        /// <summary>The party's tax identity, or null when none has been recorded yet.</summary>
        Task<PartyTaxProfile?> GetPartyTaxProfile(int userId);

        /// <summary>Drop every tax cache. Called after any master write.</summary>
        void InvalidateCache();
    }
}
