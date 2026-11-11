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

        public async Task<List<PartyServiceRelationship>> GetRelationshipsForParty(int userId) =>
            await _context.PartyServiceRelationships.AsNoTracking()
                .Where(x => x.UserID == userId && !x.IsDeleted)
                .OrderByDescending(x => x.EffectiveFrom).ThenBy(x => x.ServiceID)
                .ToListAsync();

        public async Task<(int PartyTaxProfileID, string FailureMessage)> SavePartyTaxProfile(PartyTaxProfile profile)
        {
            if (profile.UserID <= 0) return (0, "A party is required.");

            string gstin = (profile.GSTIN ?? string.Empty).Trim();
            if (gstin.Length > 0 && gstin.Length != 15)
                return (0, $"GSTIN '{gstin}' is {gstin.Length} characters; a GSTIN is 15.");

            string state = (profile.PlaceOfSupplyStateCode ?? string.Empty).Trim();
            // The first two digits of a GSTIN ARE its state code. A mismatch sends the tax to the
            // wrong head, and the wrong head is only fixed by a refund.
            if (gstin.Length == 15 && state.Length == 2 && !gstin.StartsWith(state, StringComparison.Ordinal))
                return (0, $"Place of supply '{state}' does not match the GSTIN, which begins '{gstin[..2]}'. "
                         + "One of the two is wrong.");

            var existing = await _context.PartyTaxProfiles
                .FirstOrDefaultAsync(x => x.UserID == profile.UserID && !x.IsDeleted);

            if (existing == null)
            {
                profile.GSTIN = gstin;
                profile.PlaceOfSupplyStateCode = state;
                profile.DateTime = DateTime.UtcNow;
                profile.UpdatedateTime = DateTime.UtcNow;
                _context.PartyTaxProfiles.Add(profile);
                await _context.SaveChangesAsync();
                InvalidateCache();
                return (profile.PartyTaxProfileID, string.Empty);
            }

            existing.GSTIN = gstin;
            existing.RegistrationStatus = (profile.RegistrationStatus ?? string.Empty).Trim().ToUpperInvariant();
            existing.PAN = (profile.PAN ?? string.Empty).Trim().ToUpperInvariant();
            existing.PlaceOfSupplyStateCode = state;
            existing.Is206ABFlagged = profile.Is206ABFlagged;
            existing.IsExchangePartner = profile.IsExchangePartner;
            existing.ExchangeAgreementRef = (profile.ExchangeAgreementRef ?? string.Empty).Trim();
            existing.LowerDeductionCertNo = (profile.LowerDeductionCertNo ?? string.Empty).Trim();
            existing.LDCRate = profile.LDCRate;
            existing.LDCValidFrom = profile.LDCValidFrom;
            existing.LDCValidTill = profile.LDCValidTill;
            existing.LDCAmountCap = profile.LDCAmountCap;
            existing.Remark = (profile.Remark ?? string.Empty).Trim();
            existing.UpdatedateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            InvalidateCache();
            return (existing.PartyTaxProfileID, string.Empty);
        }

        public async Task<(int PartyServiceRelationshipID, string FailureMessage)> SaveRelationship(
            PartyServiceRelationship relationship)
        {
            if (relationship.UserID <= 0) return (0, "A party is required.");

            string type = (relationship.RelationshipType ?? string.Empty).Trim().ToUpperInvariant();
            if (!Array.Exists(PartyRelationship.All, x => x == type))
                return (0, $"RelationshipType must be {string.Join(" or ", PartyRelationship.All)}. "
                         + "P2P means a discount and no 194H; AGENT means commission and 194H applies.");

            if (string.IsNullOrWhiteSpace(relationship.AgreementRef))
                return (0, "An agreement reference is required. Principal-to-principal treatment rests on what "
                         + "the contract says, and a row with nothing behind it is an assertion.");

            if (relationship.EffectiveFrom == default)
                return (0, "EffectiveFrom is required - the relationship is resolved as of the transaction date.");

            // Close any open window for the same party and service rather than editing it: whether
            // 194H applied is a question about the day, not about today.
            var open = await _context.PartyServiceRelationships
                .Where(x => x.UserID == relationship.UserID
                         && x.ServiceID == relationship.ServiceID
                         && !x.IsDeleted && x.IsActive && x.EffectiveTo == null)
                .ToListAsync();

            foreach (var row in open)
            {
                if (relationship.EffectiveFrom.Date <= row.EffectiveFrom.Date)
                    return (0, $"An existing window for this party and service begins "
                             + $"{row.EffectiveFrom:dd-MMM-yyyy}. The new one must start after it, or two windows "
                             + "would give two answers for the same day.");
                row.EffectiveTo = relationship.EffectiveFrom.Date.AddDays(-1);
                row.UpdatedateTime = DateTime.UtcNow;
            }

            relationship.RelationshipType = type;
            relationship.AgreementRef = relationship.AgreementRef.Trim();
            relationship.EffectiveFrom = relationship.EffectiveFrom.Date;
            relationship.EffectiveTo = relationship.EffectiveTo?.Date;
            relationship.IsActive = true;
            relationship.DateTime = DateTime.UtcNow;
            relationship.UpdatedateTime = DateTime.UtcNow;

            _context.PartyServiceRelationships.Add(relationship);
            await _context.SaveChangesAsync();
            InvalidateCache();
            return (relationship.PartyServiceRelationshipID, string.Empty);
        }

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
