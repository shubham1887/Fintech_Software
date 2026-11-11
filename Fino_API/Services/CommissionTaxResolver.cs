using FintechApp.Data;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Responses.CommissionTax;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="ICommissionTaxResolver" />
    public class CommissionTaxResolver : ICommissionTaxResolver
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaxConfigurationRepository _taxConfig;
        private readonly Logger _logger = new();

        public CommissionTaxResolver(ApplicationDbContext context, ITaxConfigurationRepository taxConfig)
        {
            _context = context;
            _taxConfig = taxConfig;
        }

        public async Task<CommissionTaxTreatment> Resolve(
            int serviceId, int operatorId, int providerId, int slabMasterId, DateTime asOf)
        {
            DateTime on = asOf.Date;

            var candidates = await _context.CommissionTaxProfiles.AsNoTracking()
                .Where(p => !p.IsDeleted && p.IsActive
                         && p.EffectiveFrom <= on
                         && (p.EffectiveTo == null || p.EffectiveTo >= on))
                .ToListAsync();

            // Narrow to the rows that actually apply to this transaction, then take the most
            // specific. A row that pins a service or an operator only matches that one; a row that
            // leaves them 0 matches anything, which is what makes one GLOBAL row enough to start.
            var matching = candidates
                .Where(p => Applies(p.Scope, p.ScopeID, serviceId, providerId, slabMasterId))
                .Where(p => p.ServiceID == 0 || p.ServiceID == serviceId)
                .Where(p => p.OperatorID == 0 || p.OperatorID == operatorId)
                .OrderByDescending(p => p.Priority > 0 ? p.Priority : DerivedPriority(p.Scope, p.ServiceID, p.OperatorID))
                .ThenByDescending(p => p.EffectiveFrom)
                .ThenByDescending(p => p.CommissionTaxProfileID)
                .ToList();

            var chosen = matching.FirstOrDefault();

            if (chosen == null)
            {
                // Nothing configured at all — not even the GLOBAL seed. Fall back to what this
                // install did implicitly before any of this existed, and SAY so: a silent default
                // is how the figures were wrong in the first place.
                return new CommissionTaxTreatment
                {
                    Scope = CommissionTaxScope.Global,
                    ScopeLabel = "built-in fallback",
                    TaxMode = CommissionTaxMode.Inclusive,
                    TaxBase = CommissionTaxBase.Commission,
                    GSTRatePercent = await EngineRate(serviceId, operatorId, providerId, on),
                    RateFromTaxEngine = true,
                    PerTransactionChargeTaxMode = CommissionTaxMode.Inclusive,
                    TDSEnabled = false,
                    Warning = "No commission tax profile is configured, so the built-in default was used "
                            + "(inclusive of GST, charged on the commission, TDS off). Set one on the "
                            + "Commission Tax screen.",
                };
            }

            decimal rate = chosen.GSTRatePercent;
            bool fromEngine = rate <= 0m;
            if (fromEngine) rate = await EngineRate(serviceId, operatorId, providerId, on);

            return new CommissionTaxTreatment
            {
                CommissionTaxProfileID = chosen.CommissionTaxProfileID,
                Scope = chosen.Scope,
                ScopeLabel = await ScopeLabel(chosen.Scope, chosen.ScopeID),
                TaxMode = Norm(chosen.TaxMode, CommissionTaxMode.Inclusive),
                TaxBase = Norm(chosen.TaxBase, CommissionTaxBase.Commission),
                GSTRatePercent = rate,
                RateFromTaxEngine = fromEngine,
                PerTransactionCharge = chosen.PerTransactionCharge,
                PerTransactionChargeTaxMode = Norm(chosen.PerTransactionChargeTaxMode, CommissionTaxMode.Inclusive),
                TDSEnabled = chosen.TDSEnabled,
                TDSSectionCode = chosen.TDSSectionCode ?? string.Empty,
                TDSRatePercent = chosen.TDSRatePercent,
            };
        }

        public CommissionSplit Split(decimal commission, CommissionTaxTreatment treatment, decimal grossAmount)
        {
            string mode = Norm(treatment.TaxMode, CommissionTaxMode.Inclusive);
            string basis = Norm(treatment.TaxBase, CommissionTaxBase.Commission);
            decimal rate = treatment.GSTRatePercent;

            // What the tax is charged ON. For GROSS the commission is still the income, but the
            // tax follows the whole throughput — the trading case, where talktime is bought and
            // resold rather than earned on.
            decimal taxableBase = basis == CommissionTaxBase.Gross ? grossAmount : commission;

            var split = new CommissionSplit
            {
                CommissionAsQuoted = R2(commission),
                TaxableBase = R2(taxableBase),
                TaxMode = mode,
                TaxBase = basis,
                RatePercent = rate,
            };

            if (mode == CommissionTaxMode.None || rate <= 0m)
            {
                split.NetCommission = R2(commission);
                split.GST = 0m;
                split.Payable = R2(commission);
                return split;
            }

            if (mode == CommissionTaxMode.Inclusive)
            {
                // The figure already contains the tax, so back it out. Rs 15 inclusive at 18% is
                // Rs 12.71 of income and Rs 2.29 of GST — the retailer still pays Rs 15.
                if (basis == CommissionTaxBase.Gross)
                {
                    // The tax sits inside the GROSS amount, not inside the commission, so the
                    // commission is untouched income and the tax is carved out of the throughput.
                    decimal netGross = R2(grossAmount / (1m + rate / 100m));
                    split.GST = R2(grossAmount - netGross);
                    split.NetCommission = R2(commission);
                    split.Payable = R2(commission);
                    split.TaxableBase = netGross;
                }
                else
                {
                    split.NetCommission = R2(commission / (1m + rate / 100m));
                    split.GST = R2(commission - split.NetCommission);
                    split.Payable = R2(commission);
                    split.TaxableBase = split.NetCommission;
                }
                return split;
            }

            // EXCLUSIVE: the tax is added on top. A provider quoting Rs 100 commission exclusive
            // actually owes Rs 118, and booking only Rs 100 understates the income AND the tax.
            split.NetCommission = R2(commission);
            split.GST = R2(taxableBase * rate / 100m);
            split.Payable = R2(split.NetCommission + split.GST);
            return split;
        }

        // ── Matching ──────────────────────────────────────────────────────────────────────

        private static bool Applies(string scope, int scopeId, int serviceId, int providerId, int slabMasterId) =>
            (scope ?? string.Empty).ToUpperInvariant() switch
            {
                CommissionTaxScope.Global => true,
                CommissionTaxScope.Service => scopeId == serviceId && serviceId > 0,
                CommissionTaxScope.Provider => scopeId == providerId && providerId > 0,
                CommissionTaxScope.Slab => scopeId == slabMasterId && slabMasterId > 0,
                _ => false,
            };

        /// <summary>
        /// How specific a row is, when the admin left Priority at 0. Scope first, then whether it
        /// pinned a service and an operator — so nobody has to invent priority numbers to get the
        /// obvious behaviour.
        /// </summary>
        private static int DerivedPriority(string scope, int serviceId, int operatorId)
        {
            int p = CommissionTaxScope.Rank(scope) * 10;
            if (serviceId > 0) p += 3;
            if (operatorId > 0) p += 2;
            return p;
        }

        /// <summary>
        /// The GST rate the tax engine would use for this sale, as of the date. Read from the
        /// rules rather than assumed, so a rate change is effective-dated in ONE place.
        /// </summary>
        private async Task<decimal> EngineRate(int serviceId, int operatorId, int providerId, DateTime on)
        {
            try
            {
                var rules = await _taxConfig.GetRules();
                var rates = await _taxConfig.GetRates();

                var rule = rules
                    .Where(r => r.IsActive && !r.IsDeleted
                             && r.Direction == TaxDirection.Sale
                             && (r.ServiceID == 0 || r.ServiceID == serviceId)
                             && (r.OperatorID == 0 || r.OperatorID == operatorId)
                             && (r.ProviderID == 0 || r.ProviderID == providerId)
                             && r.EffectiveFrom.Date <= on
                             && (!r.EffectiveTo.HasValue || r.EffectiveTo.Value.Date >= on))
                    .OrderByDescending(r => r.Priority)
                    .ThenByDescending(r => r.EffectiveFrom)
                    .FirstOrDefault();

                if (rule?.TaxRateMasterID == null) return 0m;
                var rateRow = rates.FirstOrDefault(x => x.TaxRateMasterID == rule.TaxRateMasterID.Value);
                // The GST rate only. Cess is a separate head and is not part of a commission rate.
                return rateRow?.Rate ?? 0m;
            }
            catch (Exception ex)
            {
                // A rate we cannot read is not a reason to fail a settlement that has already moved
                // money. Zero means "no tax applied", and the treatment reports where it came from.
                _logger.Write(ex);
                return 0m;
            }
        }

        private async Task<string> ScopeLabel(string scope, int scopeId)
        {
            try
            {
                return (scope ?? string.Empty).ToUpperInvariant() switch
                {
                    CommissionTaxScope.Service =>
                        await _context.ServiceMasters.AsNoTracking()
                            .Where(s => s.ServiceID == scopeId).Select(s => s.Name).FirstOrDefaultAsync()
                        ?? $"service #{scopeId}",
                    CommissionTaxScope.Provider =>
                        await _context.ProviderMasters.AsNoTracking()
                            .Where(p => p.ProviderID == scopeId).Select(p => p.ProviderName).FirstOrDefaultAsync()
                        ?? $"provider #{scopeId}",
                    CommissionTaxScope.Slab =>
                        await _context.SlabMasters.AsNoTracking()
                            .Where(s => s.SlabMasterID == scopeId).Select(s => s.SlabName).FirstOrDefaultAsync()
                        ?? $"slab #{scopeId}",
                    _ => "house default",
                };
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return scope ?? string.Empty;
            }
        }

        private static string Norm(string? v, string fallback)
        {
            string s = (v ?? string.Empty).Trim().ToUpperInvariant();
            return s.Length == 0 ? fallback : s;
        }

        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
