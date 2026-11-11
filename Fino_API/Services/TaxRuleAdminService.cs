using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TaxEngine;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FintechApp.Services
{
    /// <summary>See <see cref="ITaxRuleAdminService"/>.</summary>
    public class TaxRuleAdminService : ITaxRuleAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaxConfigurationRepository _config;

        public TaxRuleAdminService(ApplicationDbContext context, ITaxConfigurationRepository config)
        {
            _context = context;
            _config = config;
        }

        public async Task<List<TaxRuleMaster>> GetRules(string direction, int serviceId, bool activeOnly)
        {
            var q = _context.TaxRuleMasters.AsNoTracking().Where(r => !r.IsDeleted);
            if (!string.IsNullOrWhiteSpace(direction)) q = q.Where(r => r.Direction == direction);
            if (serviceId > 0) q = q.Where(r => r.ServiceID == serviceId);
            if (activeOnly) q = q.Where(r => r.IsActive && r.EffectiveTo == null);
            return await q.OrderBy(r => r.Direction).ThenBy(r => r.ServiceID)
                          .ThenByDescending(r => r.Priority).ToListAsync();
        }

        public async Task<TaxRuleMaster?> GetRule(int id) =>
            await _context.TaxRuleMasters.AsNoTracking().FirstOrDefaultAsync(r => r.TaxRuleMasterID == id && !r.IsDeleted);

        public async Task<List<TaxRuleAudit>> GetAudit(int id) =>
            await _context.TaxRuleAudits.AsNoTracking()
                .Where(a => a.EntityName == nameof(TaxRuleMaster) && a.EntityID == id)
                .OrderByDescending(a => a.ChangedOn).ToListAsync();

        public async Task<(int TaxRuleMasterID, string FailureMessage)> Create(SaveTaxRuleRequest request, int changedByUserID)
        {
            string invalid = Validate(request);
            if (invalid.Length > 0) return (0, invalid);

            var rule = Map(new TaxRuleMaster(), request);
            rule.Priority = request.Priority > 0 ? request.Priority : DerivePriority(rule);
            rule.DateTime = DateTime.UtcNow;
            rule.UpdatedateTime = DateTime.UtcNow;

            _context.TaxRuleMasters.Add(rule);
            await _context.SaveChangesAsync();

            await WriteAudit(rule.TaxRuleMasterID, "CREATE", null, rule, request.Reason, changedByUserID);
            _config.InvalidateCache();
            return (rule.TaxRuleMasterID, string.Empty);
        }

        public async Task<(int NewTaxRuleMasterID, string FailureMessage)> Supersede(SaveTaxRuleRequest request, int changedByUserID)
        {
            string invalid = Validate(request);
            if (invalid.Length > 0) return (0, invalid);

            var current = await _context.TaxRuleMasters
                .FirstOrDefaultAsync(r => r.TaxRuleMasterID == request.TaxRuleMasterID && !r.IsDeleted);
            if (current == null) return (0, $"Rule {request.TaxRuleMasterID} not found.");

            if (request.EffectiveFrom.Date <= current.EffectiveFrom.Date)
                return (0, $"The replacement starts {request.EffectiveFrom:dd-MMM-yyyy}, which is not after the "
                         + $"current rule began ({current.EffectiveFrom:dd-MMM-yyyy}). Overlapping windows would "
                         + "give two answers for the same day.");

            var before = Snapshot(current);

            // Close the old one the day before the new one starts, rather than editing it. Every
            // transaction that already resolved through it keeps resolving through it.
            current.EffectiveTo = request.EffectiveFrom.Date.AddDays(-1);
            current.UpdatedateTime = DateTime.UtcNow;

            var replacement = Map(new TaxRuleMaster(), request);
            replacement.Priority = request.Priority > 0 ? request.Priority : DerivePriority(replacement);
            replacement.DateTime = DateTime.UtcNow;
            replacement.UpdatedateTime = DateTime.UtcNow;
            _context.TaxRuleMasters.Add(replacement);

            await _context.SaveChangesAsync();

            await WriteAudit(current.TaxRuleMasterID, "SUPERSEDE", before, current,
                $"{request.Reason} (replaced by rule {replacement.TaxRuleMasterID})", changedByUserID);
            await WriteAudit(replacement.TaxRuleMasterID, "CREATE", null, replacement,
                $"{request.Reason} (replaces rule {current.TaxRuleMasterID})", changedByUserID);

            _config.InvalidateCache();
            return (replacement.TaxRuleMasterID, string.Empty);
        }

        public async Task<string> Deactivate(int id, DateTime effectiveTo, string reason, int changedByUserID)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return "A reason is required. A rule withdrawn without one cannot be explained later.";

            var rule = await _context.TaxRuleMasters.FirstOrDefaultAsync(r => r.TaxRuleMasterID == id && !r.IsDeleted);
            if (rule == null) return $"Rule {id} not found.";

            var before = Snapshot(rule);
            rule.EffectiveTo = effectiveTo.Date;
            rule.IsActive = false;
            rule.UpdatedateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await WriteAudit(id, "DEACTIVATE", before, rule, reason, changedByUserID);
            _config.InvalidateCache();
            return string.Empty;
        }

        // ── helpers ─────────────────────────────────────────────────────────────────────

        private static string Validate(SaveTaxRuleRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Reason))
                return "A reason is required. A rule change nobody can explain is a rule change nobody can defend.";
            if (!string.Equals(r.Direction, TaxDirection.Sale, StringComparison.OrdinalIgnoreCase)
             && !string.Equals(r.Direction, TaxDirection.Purchase, StringComparison.OrdinalIgnoreCase))
                return "Direction must be SALE or PURCHASE.";
            if (!BillingBasis.IsValid(r.BillingBasis))
                return $"BillingBasis must be one of {string.Join(", ", BillingBasis.All)}.";
            if (r.EffectiveFrom == default)
                return "EffectiveFrom is required - a rule with no start date cannot be resolved by date.";
            if (r.EffectiveTo.HasValue && r.EffectiveTo.Value.Date < r.EffectiveFrom.Date)
                return "EffectiveTo cannot be before EffectiveFrom.";
            // A taxable basis with no rate would silently charge nothing.
            bool taxable = r.BillingBasis.Equals(BillingBasis.Gross, StringComparison.OrdinalIgnoreCase)
                        || r.BillingBasis.Equals(BillingBasis.Commission, StringComparison.OrdinalIgnoreCase)
                        || r.BillingBasis.Equals(BillingBasis.Charge, StringComparison.OrdinalIgnoreCase);
            if (taxable && !r.TaxRateMasterID.HasValue)
                return $"A {r.BillingBasis} rule needs a rate, or it would charge nothing and say nothing.";
            return string.Empty;
        }

        private static TaxRuleMaster Map(TaxRuleMaster rule, SaveTaxRuleRequest r)
        {
            rule.RuleName = r.RuleName.Trim();
            rule.Direction = r.Direction.Trim().ToUpperInvariant();
            rule.ServiceID = r.ServiceID;
            rule.OperatorID = r.OperatorID;
            rule.ProviderID = r.ProviderID;
            rule.UserTypeID = r.UserTypeID;
            rule.PartyUserID = r.PartyUserID;
            rule.PartyRegistrationStatus = (r.PartyRegistrationStatus ?? string.Empty).Trim().ToUpperInvariant();
            rule.SupplyNature = (r.SupplyNature ?? string.Empty).Trim().ToUpperInvariant();
            rule.BillingBasis = r.BillingBasis.Trim().ToUpperInvariant();
            rule.TaxRateMasterID = r.TaxRateMasterID;
            rule.HSNSACMasterID = r.HSNSACMasterID;
            rule.TDSSectionMasterID = r.TDSSectionMasterID;
            rule.ReverseCharge = r.ReverseCharge;
            rule.ITCEligible = r.ITCEligible;
            rule.Remark = (r.Remark ?? string.Empty).Trim();
            rule.EffectiveFrom = r.EffectiveFrom.Date;
            rule.EffectiveTo = r.EffectiveTo?.Date;
            rule.IsActive = true;
            return rule;
        }

        /// <summary>
        /// Specificity as a priority, matching how the engine breaks ties. Derived rather than
        /// typed in, because a rule naming one party that someone forgot to prioritise would
        /// otherwise lose to a service default and nobody would know why.
        /// </summary>
        private static int DerivePriority(TaxRuleMaster r)
        {
            int p = 10;                                   // company fallback
            if (r.ServiceID != 0) p = 100;
            if (r.OperatorID != 0) p = 200;
            if (r.ProviderID != 0) p = 300;
            if (r.UserTypeID != 0) p += 10;
            if (!string.IsNullOrWhiteSpace(r.PartyRegistrationStatus)) p += 5;
            if (r.PartyUserID != 0) p = 500;               // one named party beats everything else
            return p;
        }

        private static object Snapshot(TaxRuleMaster r) => new
        {
            r.TaxRuleMasterID, r.RuleName, r.Direction, r.ServiceID, r.OperatorID, r.ProviderID,
            r.UserTypeID, r.PartyUserID, r.PartyRegistrationStatus, r.SupplyNature, r.BillingBasis,
            r.TaxRateMasterID, r.HSNSACMasterID, r.TDSSectionMasterID, r.ReverseCharge, r.ITCEligible,
            r.Priority, r.EffectiveFrom, r.EffectiveTo, r.IsActive,
        };

        private async Task WriteAudit(int entityId, string action, object? before, TaxRuleMaster after,
                                      string reason, int changedByUserID)
        {
            _context.TaxRuleAudits.Add(new TaxRuleAudit
            {
                EntityName = nameof(TaxRuleMaster),
                EntityID = entityId,
                Action = action,
                OldValueJson = before == null ? string.Empty : JsonConvert.SerializeObject(before),
                NewValueJson = JsonConvert.SerializeObject(Snapshot(after)),
                Reason = (reason ?? string.Empty).Trim(),
                ChangedByUserID = changedByUserID,
                ChangedOn = DateTime.UtcNow,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync();
        }
    }
}
