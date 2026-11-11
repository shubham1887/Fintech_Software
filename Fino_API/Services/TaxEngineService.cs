using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.TaxEngine;
using FintechApp.Responses.TaxEngine;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// The tax engine. See <see cref="ITaxEngineService"/> for why it never throws.
    ///
    /// Shape of the work:
    ///   1. Collect every rule that matches the context and is in force on the transaction date.
    ///   2. Rank them and take the most specific. A tie is a configuration mistake, so the engine
    ///      picks deterministically AND records a warning rather than choosing in silence.
    ///   3. Turn the basis into a taxable value — gross, commission or charge.
    ///   4. If the value already includes tax, work the tax back OUT of it.
    ///   5. Split by place of supply: equal states give CGST+SGST, different states give IGST.
    ///   6. Decide TDS, which turns on the party relationship and an annual threshold.
    /// </summary>
    public class TaxEngineService : ITaxEngineService
    {
        private readonly ITaxConfigurationRepository _config;

        public TaxEngineService(ITaxConfigurationRepository config) => _config = config;

        /// <summary>Commercial rounding to paise. Every stored money value goes through this, so
        /// an invoice total always equals the sum of its printed lines.</summary>
        private static decimal R2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        public async Task<TaxResolutionResult> Resolve(TaxResolutionContext ctx)
        {
            var result = new TaxResolutionResult
            {
                Direction = ctx.Direction,
                GrossValue = R2(ctx.GrossValue),
                SupplierStateCode = Norm(ctx.SupplierStateCode),
                PlaceOfSupplyStateCode = Norm(ctx.PlaceOfSupplyStateCode),
                PartyRelationship = ctx.PartyRelationship ?? string.Empty,
                ResolvedOn = DateTime.UtcNow,
            };
            var warnings = new List<string>();

            var rules = await _config.GetRules();
            var rates = await _config.GetRates();
            var sacs = await _config.GetHSNSAC();

            // ── 1. Candidates ──────────────────────────────────────────────────────────
            var matches = rules
                .Where(r => r.IsActive
                         && string.Equals(r.Direction, ctx.Direction, StringComparison.OrdinalIgnoreCase)
                         && InWindow(r.EffectiveFrom, r.EffectiveTo, ctx.TransactionDate)
                         && AnyOrEqual(r.ServiceID, ctx.ServiceID)
                         && AnyOrEqual(r.OperatorID, ctx.OperatorID)
                         && AnyOrEqual(r.ProviderID, ctx.ProviderID)
                         && AnyOrEqual(r.UserTypeID, ctx.UserTypeID)
                         && AnyOrEqual(r.PartyUserID, ctx.PartyUserID)
                         && AnyOrEqualText(r.PartyRegistrationStatus, ctx.PartyRegistrationStatus))
                .ToList();

            // ── 2. Pick ────────────────────────────────────────────────────────────────
            // Order by stored priority, then by specificity as a tie-break (a rule naming a party
            // is more specific than one naming only a service even if both were saved at the same
            // priority), then by id so the choice is stable across calls and across servers.
            var ranked = matches
                .OrderByDescending(r => r.Priority)
                .ThenByDescending(Specificity)
                .ThenBy(r => r.TaxRuleMasterID)
                .ToList();

            TaxRuleMaster? rule = ranked.FirstOrDefault();

            if (rule == null)
            {
                warnings.Add($"No tax rule matched (direction {ctx.Direction}, service {ctx.ServiceID}). "
                           + "Nothing was charged. Add a rule or a company fallback.");
                result.BillingBasis = BillingBasis.Exempt;
                result.TaxableValue = 0;
                result.Warning = string.Join(" | ", warnings);
                return result;
            }

            if (ranked.Count > 1
                && ranked[1].Priority == rule.Priority
                && Specificity(ranked[1]) == Specificity(rule))
            {
                warnings.Add($"Rules {rule.TaxRuleMasterID} and {ranked[1].TaxRuleMasterID} match equally "
                           + $"(priority {rule.Priority}). Rule {rule.TaxRuleMasterID} was used. "
                           + "Two treatments for one transaction is a configuration conflict — fix the priorities.");
            }

            result.TaxRuleMasterID = rule.TaxRuleMasterID;
            result.RuleName = rule.RuleName;
            result.BillingBasis = rule.BillingBasis;
            result.ITCEligible = rule.ITCEligible;
            result.ReverseCharge = rule.ReverseCharge;

            var sac = sacs.FirstOrDefault(s => s.HSNSACMasterID == rule.HSNSACMasterID);
            result.SACCode = sac?.Code ?? string.Empty;
            if (sac == null) warnings.Add($"Rule {rule.TaxRuleMasterID} has no SAC code; the invoice line will print none.");

            // Build the simulator's explanation from the same ordering the engine used.
            foreach (var m in ranked.Take(10))
            {
                var mRate = rates.FirstOrDefault(x => x.TaxRateMasterID == m.TaxRateMasterID);
                result.Candidates.Add(new TaxRuleMatch
                {
                    TaxRuleMasterID = m.TaxRuleMasterID,
                    RuleName = m.RuleName,
                    Priority = m.Priority,
                    BillingBasis = m.BillingBasis,
                    Rate = mRate?.Rate ?? 0,
                    SACCode = sacs.FirstOrDefault(s => s.HSNSACMasterID == m.HSNSACMasterID)?.Code ?? string.Empty,
                    MatchedOn = DescribeMatch(m),
                    IsSelected = m.TaxRuleMasterID == rule.TaxRuleMasterID,
                });
            }

            // ── 3. Basis -> the value tax is charged on ────────────────────────────────
            decimal basisValue = rule.BillingBasis.ToUpperInvariant() switch
            {
                BillingBasis.Gross => ctx.GrossValue,
                BillingBasis.Commission => ctx.CommissionValue,
                BillingBasis.Charge => ctx.ChargeValue,
                _ => 0m,
            };

            bool taxable = rule.BillingBasis.Equals(BillingBasis.Gross, StringComparison.OrdinalIgnoreCase)
                        || rule.BillingBasis.Equals(BillingBasis.Commission, StringComparison.OrdinalIgnoreCase)
                        || rule.BillingBasis.Equals(BillingBasis.Charge, StringComparison.OrdinalIgnoreCase);

            // ── 4 & 5. Rate, inclusive/exclusive, and the split ────────────────────────
            var rate = rates.FirstOrDefault(x => x.TaxRateMasterID == rule.TaxRateMasterID
                                              && InWindow(x.EffectiveFrom, x.EffectiveTo, ctx.TransactionDate));
            if (taxable && rate == null)
            {
                warnings.Add($"Rule {rule.TaxRuleMasterID} points at no rate in force on "
                           + $"{ctx.TransactionDate:yyyy-MM-dd}; tax was taken as 0.");
            }

            decimal pct = taxable ? (rate?.Rate ?? 0m) : 0m;
            result.SupplyNature = DecideSupplyNature(rule, result.SupplierStateCode, result.PlaceOfSupplyStateCode, warnings);

            decimal taxableValue;
            decimal totalTax;
            if (!taxable || pct <= 0m)
            {
                taxableValue = R2(basisValue);
                totalTax = 0m;
            }
            else if (ctx.IsTaxInclusive)
            {
                // The value already contains the tax, so back it out. This mirrors how the
                // suppliers' own invoices are built: Rs 5,50,000 inclusive at 18% shows
                // 4,66,101.69 taxable + 83,898.30 tax, and the missing paise becomes "Round Off".
                taxableValue = R2(basisValue / (1m + pct / 100m));
                totalTax = R2(taxableValue * pct / 100m);
                result.RoundOff = R2(basisValue - taxableValue - totalTax);
            }
            else
            {
                taxableValue = R2(basisValue);
                totalTax = R2(taxableValue * pct / 100m);
            }

            result.TaxableValue = taxableValue;

            if (result.SupplyNature == SupplyNature.Intrastate)
            {
                result.CGSTRate = pct / 2m;
                result.SGSTRate = pct / 2m;
                // Split the AMOUNT rather than computing each half from the rate, so the two
                // halves always add back to the total instead of drifting a paisa apart.
                result.CGSTAmount = R2(totalTax / 2m);
                result.SGSTAmount = R2(totalTax - result.CGSTAmount);
            }
            else if (result.SupplyNature == SupplyNature.Export || result.SupplyNature == SupplyNature.SEZ)
            {
                // Zero-rated. The turnover still has to be reported, so the taxable value stands
                // and only the tax falls away.
                totalTax = 0m;
                result.RoundOff = 0m;
            }
            else
            {
                result.IGSTRate = pct;
                result.IGSTAmount = totalTax;
            }

            result.CessAmount = (taxable && rate != null && rate.CessRate > 0)
                ? R2(taxableValue * rate.CessRate / 100m)
                : 0m;

            result.TotalTax = R2(result.CGSTAmount + result.SGSTAmount + result.IGSTAmount + result.CessAmount);
            result.InvoiceValue = R2(result.TaxableValue + result.TotalTax + result.RoundOff);

            // ── 6. TDS ─────────────────────────────────────────────────────────────────
            if (rule.TDSSectionMasterID.HasValue)
            {
                await ApplyTds(rule, ctx, result, warnings);
            }

            result.Warning = string.Join(" | ", warnings);
            return result;
        }

        /// <summary>
        /// TDS, which turns entirely on the relationship.
        ///
        /// Bharti Cellular (SC, 2024): a discount to a prepaid distributor is not commission,
        /// because the relationship is principal-to-principal rather than principal-agent, so
        /// Section 194H does not apply. So P2P deducts nothing, AGENT deducts, and an UNKNOWN
        /// relationship deducts nothing and says so — guessing either over-deducts from a
        /// distributor or leaves a real agent short-deducted, and both cost money.
        /// </summary>
        private async Task ApplyTds(TaxRuleMaster rule, TaxResolutionContext ctx,
                                    TaxResolutionResult result, List<string> warnings)
        {
            var sections = await _config.GetTDSSections();
            var section = sections.FirstOrDefault(s => s.TDSSectionMasterID == rule.TDSSectionMasterID
                                                    && InWindow(s.EffectiveFrom, s.EffectiveTo, ctx.TransactionDate));
            if (section == null)
            {
                warnings.Add($"Rule {rule.TaxRuleMasterID} names a TDS section with no version in force on "
                           + $"{ctx.TransactionDate:yyyy-MM-dd}; no TDS was deducted.");
                return;
            }

            string relationship = (ctx.PartyRelationship ?? string.Empty).Trim().ToUpperInvariant();
            if (relationship.Length == 0)
            {
                warnings.Add($"No relationship recorded for party {ctx.PartyUserID} on service {ctx.ServiceID}, "
                           + $"so section {section.SectionCode} was not applied. Set P2P or AGENT from the "
                           + "agreement — Bharti Cellular makes this the deciding fact.");
                return;
            }

            if (relationship == PartyRelationship.PrincipalToPrincipal)
            {
                result.TDSSectionCode = string.Empty;
                result.TDSAmount = 0m;
                return;
            }

            decimal tdsBase = R2(ctx.CommissionValue);
            if (tdsBase <= 0m) return;

            // The threshold is annual PER PARTY, so the test is on the year's total including
            // this payment — not on this payment alone, which under-deducts for every party that
            // crosses the line in small amounts.
            if (ctx.PartyYtdCommission + tdsBase <= section.ThresholdAnnual)
            {
                result.TDSSectionCode = section.SectionCode;
                result.TDSRate = 0m;
                result.TDSAmount = 0m;
                return;
            }

            decimal tdsRate;
            if (ctx.PartyLDCRate > 0m) tdsRate = ctx.PartyLDCRate;                       // s.197 certificate
            else if (!ctx.PartyHasValidPAN) tdsRate = section.NoPANRate;                  // s.206AA
            else if (ctx.PartyIs206ABFlagged && section.NonFilerRate > 0) tdsRate = section.NonFilerRate;
            else tdsRate = section.Rate;

            result.TDSSectionCode = section.SectionCode;
            result.TDSRate = tdsRate;
            result.TDSAmount = R2(tdsBase * tdsRate / 100m);
        }

        // ── helpers ─────────────────────────────────────────────────────────────────────

        /// <summary>A two-digit state code. Tolerates a whole GSTIN being passed in, because its
        /// first two digits ARE the state code and that is a mistake worth absorbing rather than
        /// turning into a wrong tax head.</summary>
        private static string Norm(string? code)
        {
            string s = (code ?? string.Empty).Trim();
            if (s.Length == 0) return string.Empty;
            if (s.Length > 2) s = s.Substring(0, 2);
            return s.PadLeft(2, '0');
        }

        private static bool InWindow(DateTime from, DateTime? to, DateTime asOf) =>
            asOf.Date >= from.Date && (!to.HasValue || asOf.Date <= to.Value.Date);

        /// <summary>0 on the rule means "any", so it matches everything.</summary>
        private static bool AnyOrEqual(int ruleValue, int contextValue) =>
            ruleValue == 0 || ruleValue == contextValue;

        /// <summary>An empty string on the rule means "any".</summary>
        private static bool AnyOrEqualText(string? ruleValue, string? contextValue) =>
            string.IsNullOrWhiteSpace(ruleValue)
            || string.Equals(ruleValue.Trim(), (contextValue ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// How many dimensions a rule pins. Used only to break a tie between rules saved at the
        /// same priority — a rule naming one party is a more deliberate statement than one naming
        /// only a service, so it should win even if someone forgot to raise its priority.
        /// </summary>
        private static int Specificity(TaxRuleMaster r)
        {
            int n = 0;
            if (r.PartyUserID != 0) n += 16;
            if (r.ProviderID != 0) n += 8;
            if (r.OperatorID != 0) n += 4;
            if (r.ServiceID != 0) n += 2;
            if (r.UserTypeID != 0) n += 1;
            if (!string.IsNullOrWhiteSpace(r.PartyRegistrationStatus)) n += 1;
            return n;
        }

        private static string DescribeMatch(TaxRuleMaster r)
        {
            var parts = new List<string>();
            if (r.PartyUserID != 0) parts.Add("party");
            if (r.ProviderID != 0) parts.Add("provider");
            if (r.OperatorID != 0) parts.Add("operator");
            if (r.ServiceID != 0) parts.Add("service");
            if (r.UserTypeID != 0) parts.Add("user type");
            if (!string.IsNullOrWhiteSpace(r.PartyRegistrationStatus)) parts.Add("registration status");
            return parts.Count == 0 ? "company fallback" : string.Join(", ", parts);
        }

        /// <summary>
        /// Intra-state or inter-state. Never a choice: equal state codes give CGST+SGST,
        /// different codes give IGST. A rule may pin EXPORT or SEZ, which are zero-rated.
        ///
        /// With no place of supply on record the GST position for an unregistered recipient is
        /// the supplier's own location, i.e. intra-state. That is the defensible default, but it
        /// is also how a genuinely inter-state supply ends up under the wrong head, so it carries
        /// a warning rather than passing quietly.
        /// </summary>
        private static string DecideSupplyNature(TaxRuleMaster rule, string supplierState,
                                                  string placeOfSupply, List<string> warnings)
        {
            if (!string.IsNullOrWhiteSpace(rule.SupplyNature)) return rule.SupplyNature.ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(supplierState))
            {
                warnings.Add("Company state code is not set, so the supply was treated as intra-state. "
                           + "Set the GSTIN state on the company profile.");
                return SupplyNature.Intrastate;
            }

            if (string.IsNullOrWhiteSpace(placeOfSupply))
            {
                warnings.Add("No place of supply on record, so the supply was treated as intra-state "
                           + "(the supplier's location). Set the party's state to be sure of the tax head.");
                return SupplyNature.Intrastate;
            }

            return string.Equals(supplierState, placeOfSupply, StringComparison.OrdinalIgnoreCase)
                ? SupplyNature.Intrastate
                : SupplyNature.Interstate;
        }
    }
}
