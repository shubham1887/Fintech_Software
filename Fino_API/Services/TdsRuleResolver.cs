using FintechApp.Data;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="ITdsRuleResolver" />
    public class TdsRuleResolver : ITdsRuleResolver
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaxConfigurationRepository _taxConfig;
        private readonly ISettingMasterRepository _settings;
        private readonly Logger _logger = new();

        public TdsRuleResolver(
            ApplicationDbContext context,
            ITaxConfigurationRepository taxConfig,
            ISettingMasterRepository settings)
        {
            _context = context;
            _taxConfig = taxConfig;
            _settings = settings;
        }

        // ── The master switch ────────────────────────────────────────────────────────────────

        public async Task<bool> IsMasterSwitchOn()
        {
            try
            {
                var all = await _settings.GetAll();
                var row = all.FirstOrDefault(s => !s.IsDeleted
                                               && (s.Key ?? string.Empty).EqualsIgnoreCase(SettingKeys.TDSEnabled));
                if (row == null) return false;   // not seeded = off, which is the safe reading

                // Same truthiness the Key Setting screen writes and the other gates read.
                string v = (row.Value ?? string.Empty).Trim().ToLowerInvariant();
                if (v is "false" or "0" or "no" or "off" or "n") return false;
                if (v is "true" or "1" or "yes" or "on" or "y") return true;
                return row.IsOn;
            }
            catch (Exception ex)
            {
                // A settings read that fails must not start deducting money by accident.
                _logger.Write(ex);
                return false;
            }
        }

        // ── Which rule applies ──────────────────────────────────────────────────────────────

        public async Task<TdsDecision> Resolve(int userID, int userTypeID, int serviceID, int operatorID, DateTime asOf)
        {
            DateTime on = asOf.Date;

            var candidates = await _context.TdsRules.AsNoTracking()
                .Where(r => !r.IsDeleted && r.IsActive
                         && r.EffectiveFrom <= on
                         && (r.EffectiveTo == null || r.EffectiveTo >= on))
                .ToListAsync();

            var chosen = candidates
                .Where(r => Applies(r.Scope, r.UserID, r.UserTypeID, r.ServiceID, userID, userTypeID, serviceID))
                // An operator pinned on the rule narrows it further; 0 matches any.
                .Where(r => r.OperatorID == 0 || r.OperatorID == operatorID)
                .OrderByDescending(r => r.Priority > 0 ? r.Priority : TdsScope.Rank(r.Scope))
                .ThenByDescending(r => r.OperatorID > 0)
                .ThenByDescending(r => r.EffectiveFrom)
                .ThenByDescending(r => r.TdsRuleID)
                .FirstOrDefault();

            if (chosen == null) return new TdsDecision { RuleFound = false };

            return new TdsDecision
            {
                RuleFound = true,
                TdsRuleID = chosen.TdsRuleID,
                Scope = chosen.Scope,
                ChargeType = TdsChargeType.Normalize(chosen.ChargeType),
                ChargeValue = chosen.ChargeValue,
                ApplyOn = TdsApplyOn.Normalize(chosen.ApplyOn),
                MinCommission = chosen.MinCommission,
                MaxTDSPerTransaction = chosen.MaxTDSPerTransaction,
                SectionCode = (chosen.SectionCode ?? string.Empty).Trim(),
                ApplyNoPANRate = chosen.ApplyNoPANRate,
                DecidedBy = await Describe(chosen.Scope, chosen.UserID, chosen.UserTypeID, chosen.ServiceID,
                                           TdsChargeType.Normalize(chosen.ChargeType), chosen.ChargeValue,
                                           TdsApplyOn.Normalize(chosen.ApplyOn)),
            };
        }

        /// <summary>
        /// Does this rule address this party on this service? A scope only matches when the thing
        /// it names is actually filled in — a USER rule with UserID 0 would otherwise match
        /// everybody, which is the opposite of what somebody writing a per-user rule meant.
        /// </summary>
        private static bool Applies(
            string? scope, int ruleUser, int ruleUserType, int ruleService,
            int userID, int userTypeID, int serviceID)
            => (scope ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                TdsScope.UserService => ruleUser > 0 && ruleUser == userID && ruleService > 0 && ruleService == serviceID,
                TdsScope.User => ruleUser > 0 && ruleUser == userID,
                TdsScope.UserTypeService => ruleUserType > 0 && ruleUserType == userTypeID && ruleService > 0 && ruleService == serviceID,
                TdsScope.UserType => ruleUserType > 0 && ruleUserType == userTypeID,
                TdsScope.Service => ruleService > 0 && ruleService == serviceID,
                TdsScope.Global => true,
                _ => false,
            };

        // ── What it comes to ────────────────────────────────────────────────────────────────

        public async Task<TdsComputation> Compute(
            TdsDecision decision, int userID, decimal commissionGross, decimal commissionNet)
        {
            var result = new TdsComputation
            {
                CommissionAsEarned = R2(commissionGross),
                ChargeType = decision.ChargeType,
                DecidedBy = decision.DecidedBy,
                SectionCode = decision.SectionCode.Length > 0 ? decision.SectionCode : "194H",
            };

            if (commissionGross <= 0m)
            {
                // A negative commission is a charge the party PAID. There is no income to deduct
                // tax from, and deducting anyway would take money twice.
                result.SkipReason = "No commission was earned on this transaction, so there is nothing to deduct from.";
                return result;
            }

            decimal basis = decision.ApplyOn == TdsApplyOn.CommissionGross ? commissionGross : commissionNet;
            if (basis <= 0m) basis = commissionGross;
            result.Base = R2(basis);

            if (decision.MinCommission > 0m && commissionGross < decision.MinCommission)
            {
                result.SkipReason = $"The commission ({Money(commissionGross)}) is below the rule's minimum "
                                  + $"of {Money(decision.MinCommission)}, so nothing was deducted.";
                return result;
            }

            decimal tds;
            if (decision.ChargeType == TdsChargeType.Fix)
            {
                // A flat figure, whatever the commission is. It is still capped by the commission
                // itself: a ₹2 deduction from a ₹1.50 commission would hand somebody a negative
                // payout, and a rule that does that is a typo, not an instruction.
                tds = Math.Min(decision.ChargeValue, basis);
                result.RatePercent = 0m;
            }
            else
            {
                decimal rate = decision.ChargeValue;

                // The statutory higher rate when the party has no PAN. Only for a percentage: a
                // flat figure has no statutory twin to raise it to.
                if (decision.ApplyNoPANRate && !await HasPAN(userID))
                {
                    decimal noPan = await NoPANRate(result.SectionCode);
                    if (noPan > rate)
                    {
                        rate = noPan;
                        result.NoPANRateApplied = true;
                    }
                }

                result.RatePercent = rate;
                tds = basis * rate / 100m;
            }

            if (decision.MaxTDSPerTransaction > 0m && tds > decision.MaxTDSPerTransaction)
                tds = decision.MaxTDSPerTransaction;

            result.TDSAmount = R2(tds);

            if (result.TDSAmount <= 0m && result.SkipReason.Length == 0)
            {
                result.SkipReason = decision.ChargeType == TdsChargeType.Fix
                    ? "The rule's flat figure is 0, so nothing was deducted."
                    : "The rule's rate is 0%, so nothing was deducted.";
            }

            return result;
        }

        // ── Plumbing ────────────────────────────────────────────────────────────────────────

        /// <summary>PAN from the tax profile first, then the user record — the same order the
        /// invoicing side reads it, so one party cannot be registered in one place and not the other.</summary>
        private async Task<bool> HasPAN(int userID)
        {
            var profile = await _taxConfig.GetPartyTaxProfile(userID);
            string pan = (profile?.PAN ?? string.Empty).Trim();
            if (pan.Length == 10) return true;

            pan = (await _context.UserMasters.AsNoTracking()
                .Where(u => u.UserMasterID == userID)
                .Select(u => u.PANNo ?? string.Empty)
                .FirstOrDefaultAsync() ?? string.Empty).Trim();
            return pan.Length == 10;
        }

        private async Task<decimal> NoPANRate(string sectionCode)
        {
            var sections = await _taxConfig.GetTDSSections();
            return sections.FirstOrDefault(s => s.IsActive && !s.IsDeleted
                        && string.Equals(s.SectionCode, sectionCode, StringComparison.OrdinalIgnoreCase))?.NoPANRate ?? 0m;
        }

        /// <summary>The rule in one sentence, naming the party or role rather than an id.</summary>
        private async Task<string> Describe(
            string scope, int ruleUser, int ruleUserType, int ruleService, string chargeType, decimal value, string applyOn)
        {
            string who = scope.ToUpperInvariant() switch
            {
                TdsScope.UserService => $"{await UserName(ruleUser)} on {await ServiceName(ruleService)}",
                TdsScope.User => await UserName(ruleUser),
                TdsScope.UserTypeService => $"every {await RoleName(ruleUserType)} on {await ServiceName(ruleService)}",
                TdsScope.UserType => $"every {await RoleName(ruleUserType)}",
                TdsScope.Service => $"everyone on {await ServiceName(ruleService)}",
                _ => "the house default",
            };

            string figure = chargeType == TdsChargeType.Fix
                ? $"a flat {Money(value)} per transaction"
                : $"{value:0.####}% of the commission"
                  + (applyOn == TdsApplyOn.CommissionGross ? " as quoted" : " net of GST");

            return $"TDS rule for {who}: {figure}";
        }

        private async Task<string> UserName(int userID)
        {
            if (userID <= 0) return "a user";
            var u = await _context.UserMasters.AsNoTracking()
                .Where(x => x.UserMasterID == userID)
                .Select(x => new { x.PersonName, x.FirmName, x.AliasName })
                .FirstOrDefaultAsync();
            if (u == null) return $"user {userID}";
            string name = !string.IsNullOrWhiteSpace(u.FirmName) ? u.FirmName!
                        : !string.IsNullOrWhiteSpace(u.PersonName) ? u.PersonName! : $"user {userID}";
            return string.IsNullOrWhiteSpace(u.AliasName) ? name : $"{name} ({u.AliasName})";
        }

        private async Task<string> RoleName(int userTypeID)
        {
            if (userTypeID <= 0) return "role";
            string? name = await _context.UserTypeMasters.AsNoTracking()
                .Where(t => t.UserTypeID == userTypeID)
                .Select(t => t.UserType)
                .FirstOrDefaultAsync();
            return string.IsNullOrWhiteSpace(name) ? $"role {userTypeID}" : name!;
        }

        private async Task<string> ServiceName(int serviceID)
        {
            if (serviceID <= 0) return "every service";
            string? name = await _context.ServiceMasters.AsNoTracking()
                .Where(s => s.ServiceID == serviceID)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();
            return string.IsNullOrWhiteSpace(name) ? $"service {serviceID}" : name!;
        }

        private static string Money(decimal v) => $"Rs {v:0.00}";
        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
