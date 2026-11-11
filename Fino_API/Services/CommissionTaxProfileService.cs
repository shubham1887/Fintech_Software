using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CommissionTax;
using FintechApp.Responses.CommissionTax;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="ICommissionTaxProfileService" />
    public class CommissionTaxProfileService : ICommissionTaxProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICommissionTaxResolver _resolver;
        private readonly ITaxConfigurationRepository _taxConfig;
        private readonly Logger _logger = new();

        public CommissionTaxProfileService(
            ApplicationDbContext context, ICommissionTaxResolver resolver, ITaxConfigurationRepository taxConfig)
        {
            _context = context;
            _resolver = resolver;
            _taxConfig = taxConfig;
        }

        public async Task<List<CommissionTaxProfileRow>> GetAll(string scope)
        {
            var q = _context.CommissionTaxProfiles.AsNoTracking().Where(p => !p.IsDeleted);
            if (!string.IsNullOrWhiteSpace(scope))
                q = q.Where(p => p.Scope == scope.Trim().ToUpperInvariant());

            var rows = await q.OrderBy(p => p.Scope).ThenBy(p => p.ScopeID)
                              .ThenByDescending(p => p.EffectiveFrom).ToListAsync();
            if (rows.Count == 0) return new List<CommissionTaxProfileRow>();

            // Names in one pass, so a list of fifty rows is not fifty lookups.
            var services = await _context.ServiceMasters.AsNoTracking()
                .Select(s => new { s.ServiceID, s.Name }).ToListAsync();
            var providers = await _context.ProviderMasters.AsNoTracking()
                .Select(p => new { p.ProviderID, p.ProviderName }).ToListAsync();
            var slabs = await _context.SlabMasters.AsNoTracking()
                .Select(s => new { s.SlabMasterID, s.SlabName }).ToListAsync();
            var operators = await _context.OperatorMasters.AsNoTracking()
                .Select(o => new { o.OperatorID, o.OperatorName }).ToListAsync();

            string ScopeName(string sc, int id) => (sc ?? string.Empty).ToUpperInvariant() switch
            {
                CommissionTaxScope.Service => services.FirstOrDefault(x => x.ServiceID == id)?.Name ?? $"service #{id}",
                CommissionTaxScope.Provider => providers.FirstOrDefault(x => x.ProviderID == id)?.ProviderName ?? $"provider #{id}",
                CommissionTaxScope.Slab => slabs.FirstOrDefault(x => x.SlabMasterID == id)?.SlabName ?? $"slab #{id}",
                _ => "House default",
            };

            return rows.Select(p => new CommissionTaxProfileRow
            {
                CommissionTaxProfileID = p.CommissionTaxProfileID,
                Scope = p.Scope,
                ScopeID = p.ScopeID,
                ScopeName = ScopeName(p.Scope, p.ScopeID),
                ServiceID = p.ServiceID,
                ServiceName = p.ServiceID == 0 ? "Any"
                    : services.FirstOrDefault(x => x.ServiceID == p.ServiceID)?.Name ?? $"#{p.ServiceID}",
                OperatorID = p.OperatorID,
                OperatorName = p.OperatorID == 0 ? "Any"
                    : operators.FirstOrDefault(x => x.OperatorID == p.OperatorID)?.OperatorName ?? $"#{p.OperatorID}",
                TaxMode = p.TaxMode,
                TaxBase = p.TaxBase,
                GSTRatePercent = p.GSTRatePercent,
                PerTransactionCharge = p.PerTransactionCharge,
                PerTransactionChargeTaxMode = p.PerTransactionChargeTaxMode,
                TDSEnabled = p.TDSEnabled,
                TDSSectionCode = p.TDSSectionCode,
                TDSRatePercent = p.TDSRatePercent,
                EffectiveFrom = p.EffectiveFrom,
                EffectiveTo = p.EffectiveTo,
                Priority = p.Priority,
                Remark = p.Remark,
                IsActive = p.IsActive,
            }).ToList();
        }

        public async Task<(int ProfileID, string Failure)> Save(SaveCommissionTaxProfileRequest request)
        {
            string scope = (request.Scope ?? string.Empty).Trim().ToUpperInvariant();
            if (!CommissionTaxScope.All.Contains(scope))
                return (0, "Scope must be GLOBAL, SERVICE, PROVIDER or SLAB.");

            if (scope != CommissionTaxScope.Global && request.ScopeID <= 0)
                return (0, $"A {scope.ToLowerInvariant()} must be chosen.");

            string mode = (request.TaxMode ?? string.Empty).Trim().ToUpperInvariant();
            if (!CommissionTaxMode.All.Contains(mode))
                return (0, "Tax mode must be INCLUSIVE, EXCLUSIVE or NONE.");

            string basis = (request.TaxBase ?? string.Empty).Trim().ToUpperInvariant();
            if (!CommissionTaxBase.All.Contains(basis))
                return (0, "Tax base must be COMMISSION or GROSS.");

            string perTxnMode = (request.PerTransactionChargeTaxMode ?? string.Empty).Trim().ToUpperInvariant();
            if (perTxnMode.Length == 0) perTxnMode = CommissionTaxMode.Inclusive;
            if (!CommissionTaxMode.All.Contains(perTxnMode))
                return (0, "Per-transaction charge tax mode must be INCLUSIVE, EXCLUSIVE or NONE.");

            if (request.GSTRatePercent < 0m || request.GSTRatePercent > 100m)
                return (0, "The GST rate must be between 0 and 100. Leave it 0 to use the tax engine's rate.");
            if (request.TDSRatePercent < 0m || request.TDSRatePercent > 100m)
                return (0, "The TDS rate must be between 0 and 100. Leave it 0 to use the section's own rate.");

            if (request.EffectiveTo.HasValue && request.EffectiveTo.Value.Date < request.EffectiveFrom.Date)
                return (0, "The row ends before it starts.");

            // Only one GLOBAL row: a house default that could be two rows is not a default.
            if (scope == CommissionTaxScope.Global)
            {
                bool another = await _context.CommissionTaxProfiles.AnyAsync(p =>
                    !p.IsDeleted && p.Scope == CommissionTaxScope.Global
                    && p.CommissionTaxProfileID != request.CommissionTaxProfileID);
                if (another) return (0, "There is already a house default. Edit that one instead of adding a second.");
            }

            CommissionTaxProfile entity;
            if (request.CommissionTaxProfileID > 0)
            {
                var found = await _context.CommissionTaxProfiles
                    .FirstOrDefaultAsync(p => p.CommissionTaxProfileID == request.CommissionTaxProfileID && !p.IsDeleted);
                if (found == null) return (0, "That profile does not exist.");
                entity = found;
            }
            else
            {
                entity = new CommissionTaxProfile { DateTime = DateTime.UtcNow, IsDeleted = false };
                _context.CommissionTaxProfiles.Add(entity);
            }

            entity.Scope = scope;
            entity.ScopeID = scope == CommissionTaxScope.Global ? 0 : request.ScopeID;
            entity.ServiceID = Math.Max(0, request.ServiceID);
            entity.OperatorID = Math.Max(0, request.OperatorID);
            entity.TaxMode = mode;
            entity.TaxBase = basis;
            entity.GSTRatePercent = request.GSTRatePercent;
            entity.PerTransactionCharge = Math.Max(0m, request.PerTransactionCharge);
            entity.PerTransactionChargeTaxMode = perTxnMode;
            entity.TDSEnabled = request.TDSEnabled;
            entity.TDSSectionCode = (request.TDSSectionCode ?? string.Empty).Trim().ToUpperInvariant();
            entity.TDSRatePercent = request.TDSRatePercent;
            entity.EffectiveFrom = request.EffectiveFrom == default
                ? new DateTime(2000, 1, 1) : request.EffectiveFrom.Date;
            entity.EffectiveTo = request.EffectiveTo?.Date;
            entity.Priority = Math.Max(0, request.Priority);
            entity.Remark = Trim(request.Remark, 300);
            entity.IsActive = request.IsActive;
            entity.CreatedByUserID = request.CreatedByUserID;
            entity.UpdatedateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (entity.CommissionTaxProfileID, string.Empty);
        }

        public async Task<string> Delete(int profileId)
        {
            var entity = await _context.CommissionTaxProfiles
                .FirstOrDefaultAsync(p => p.CommissionTaxProfileID == profileId && !p.IsDeleted);
            if (entity == null) return "That profile does not exist.";

            if (entity.Scope == CommissionTaxScope.Global)
                return "The house default cannot be removed — every commission falls back to it. Edit it instead.";

            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        public async Task<(CommissionTaxPreview? Preview, string Failure)> Preview(CommissionTaxPreviewRequest request)
        {
            if (request.Commission == 0m && request.Amount == 0m)
                return (null, "Enter an amount or a commission to try.");

            DateTime asOf = (request.AsOf ?? DateTime.Today).Date;
            var treatment = await _resolver.Resolve(
                request.ServiceID, request.OperatorID, request.ProviderID, request.SlabMasterID, asOf);

            var split = _resolver.Split(request.Commission, treatment, request.Amount);

            var preview = new CommissionTaxPreview { Treatment = treatment, Split = split };

            preview.Steps.Add($"Treatment came from the {treatment.ScopeLabel} ({treatment.Scope}).");
            preview.Steps.Add(
                $"The commission is read as {Describe(treatment.TaxMode)} GST, charged on " +
                $"{(treatment.TaxBase == CommissionTaxBase.Gross ? "the whole transaction amount" : "the commission")}, " +
                $"at {treatment.GSTRatePercent}%" +
                (treatment.RateFromTaxEngine ? " (from the tax rules, not pinned here)." : " (pinned on this profile)."));

            if (treatment.TaxMode == CommissionTaxMode.Inclusive)
                preview.Steps.Add($"{Money(split.CommissionAsQuoted)} quoted = {Money(split.NetCommission)} income + {Money(split.GST)} GST carved out of it.");
            else if (treatment.TaxMode == CommissionTaxMode.Exclusive)
                preview.Steps.Add($"{Money(split.NetCommission)} income + {Money(split.GST)} GST added on top = {Money(split.Payable)} payable.");
            else
                preview.Steps.Add($"No GST applies, so {Money(split.NetCommission)} is the whole of it.");

            // TDS, which only bites once somebody turns it on.
            if (treatment.TDSEnabled && request.PartyUserID > 0 && split.NetCommission > 0m)
            {
                var sections = await _taxConfig.GetTDSSections();
                string code = treatment.TDSSectionCode.Length > 0 ? treatment.TDSSectionCode : "194H";
                var section = sections.FirstOrDefault(s =>
                    s.IsActive && !s.IsDeleted && string.Equals(s.SectionCode, code, StringComparison.OrdinalIgnoreCase));

                decimal rate = treatment.TDSRatePercent > 0m ? treatment.TDSRatePercent : section?.Rate ?? 0m;

                var profile = await _taxConfig.GetPartyTaxProfile(request.PartyUserID);
                string pan = (profile?.PAN ?? string.Empty).Trim();
                if (pan.Length == 0)
                {
                    pan = (await _context.UserMasters.AsNoTracking()
                        .Where(u => u.UserMasterID == request.PartyUserID)
                        .Select(u => u.PANNo ?? string.Empty).FirstOrDefaultAsync() ?? string.Empty).Trim();
                }

                bool noPan = pan.Length != 10 && section != null && section.NoPANRate > 0m;
                if (noPan) rate = section!.NoPANRate;

                preview.TDSSectionCode = section?.SectionCode ?? code;
                preview.TDSAmount = Math.Round(split.NetCommission * rate / 100m, 2, MidpointRounding.AwayFromZero);
                preview.Steps.Add(
                    $"TDS under {preview.TDSSectionCode} at {rate}% on the income of {Money(split.NetCommission)} " +
                    $"= {Money(preview.TDSAmount)}" +
                    (noPan ? " — the higher no-PAN rate, because this party has no PAN on record." : "."));
            }
            else if (treatment.TDSEnabled)
            {
                preview.Steps.Add("TDS is on, but no party was given, so no deduction is shown.");
            }
            else
            {
                preview.Steps.Add("TDS is off for this combination, so nothing is deducted.");
            }

            preview.NetToParty = Math.Round(split.Payable - preview.TDSAmount, 2, MidpointRounding.AwayFromZero);
            preview.Steps.Add($"The party receives {Money(preview.NetToParty)}.");

            if (!string.IsNullOrWhiteSpace(treatment.Warning)) preview.Steps.Add(treatment.Warning);

            return (preview, string.Empty);
        }

        private static string Describe(string mode) => mode switch
        {
            CommissionTaxMode.Inclusive => "INCLUSIVE of",
            CommissionTaxMode.Exclusive => "EXCLUSIVE of",
            _ => "outside",
        };

        private static string Money(decimal v) => $"Rs {v:N2}";

        private static string Trim(string? s, int max)
            => string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= max ? s : s[..max]);
    }
}
