using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Processing;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    public class TaxConfigurationRepository : ITaxConfigurationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<TaxRateMaster>> _rateCache = new();
        private readonly MemoryCacheProcessing<List<HSNSACMaster>> _sacCache = new();
        private readonly MemoryCacheProcessing<List<TDSSectionMaster>> _sectionCache = new();
        private readonly MemoryCacheProcessing<List<TaxRuleMaster>> _ruleCache = new();
        private readonly MemoryCacheProcessing<List<PartyServiceRelationship>> _relCache = new();

        public TaxConfigurationRepository(ApplicationDbContext context) => _context = context;

        public Task<List<TaxRateMaster>> GetRates() =>
            Task.FromResult(_rateCache.GetOrCreate(CacheKeys.AllTaxRates, () =>
                _context.TaxRateMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList())
                ?? new List<TaxRateMaster>());

        public Task<List<HSNSACMaster>> GetHSNSAC() =>
            Task.FromResult(_sacCache.GetOrCreate(CacheKeys.AllHSNSAC, () =>
                _context.HSNSACMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList())
                ?? new List<HSNSACMaster>());

        public Task<List<TDSSectionMaster>> GetTDSSections() =>
            Task.FromResult(_sectionCache.GetOrCreate(CacheKeys.AllTDSSections, () =>
                _context.TDSSectionMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList())
                ?? new List<TDSSectionMaster>());

        public Task<List<TaxRuleMaster>> GetRules() =>
            Task.FromResult(_ruleCache.GetOrCreate(CacheKeys.AllTaxRules, () =>
                _context.TaxRuleMasters.AsNoTracking().Where(x => !x.IsDeleted).ToList())
                ?? new List<TaxRuleMaster>());

        public Task<List<PartyServiceRelationship>> GetPartyServiceRelationships() =>
            Task.FromResult(_relCache.GetOrCreate(CacheKeys.AllPartyServiceRelationships, () =>
                _context.PartyServiceRelationships.AsNoTracking().Where(x => !x.IsDeleted).ToList())
                ?? new List<PartyServiceRelationship>());

        // Not cached as a list: profiles carry PAN, GSTIN and certificate details, there is one
        // per party, and they are read for a single party at a time. Holding every party's tax
        // identity in memory to save one indexed lookup is a poor trade.
        public async Task<PartyTaxProfile?> GetPartyTaxProfile(int userId) =>
            await _context.PartyTaxProfiles.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserID == userId && !x.IsDeleted);

        public void InvalidateCache()
        {
            _rateCache.Remove(CacheKeys.AllTaxRates);
            _sacCache.Remove(CacheKeys.AllHSNSAC);
            _sectionCache.Remove(CacheKeys.AllTDSSections);
            _ruleCache.Remove(CacheKeys.AllTaxRules);
            _relCache.Remove(CacheKeys.AllPartyServiceRelationships);
        }
    }
}
