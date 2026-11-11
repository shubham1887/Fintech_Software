using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Responses.CommissionTax;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="ITdsDeductionService" />
    public class TdsDeductionService : ITdsDeductionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICommissionTaxResolver _commissionTax;
        private readonly ITaxConfigurationRepository _taxConfig;
        private readonly ITdsRuleResolver _rules;
        private readonly IAccountProcessingService _account;
        private readonly Logger _logger = new();

        public TdsDeductionService(
            ApplicationDbContext context,
            ICommissionTaxResolver commissionTax,
            ITaxConfigurationRepository taxConfig,
            ITdsRuleResolver rules,
            IAccountProcessingService account)
        {
            _context = context;
            _commissionTax = commissionTax;
            _taxConfig = taxConfig;
            _rules = rules;
            _account = account;
        }

        public async Task<TdsDeductionResult> DeductForTransaction(int transactionMasterId)
        {
            var result = new TdsDeductionResult { TransactionMasterID = transactionMasterId };

            var txn = await _context.TransactionMasters.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TransactionMasterID == transactionMasterId);
            if (txn == null) { result.Note = "Transaction not found."; return result; }

            // Which slab the commissions came from, so a SLAB-scoped profile can be matched. The
            // transaction does not carry it, so it is read from the user's mapping.
            int slabMasterId = await _context.UserSlabMappings.AsNoTracking()
                .Where(m => m.UserID == txn.UserID && !m.IsDeleted)
                .Select(m => m.SlabMasterID).FirstOrDefaultAsync();

            var treatment = await _commissionTax.Resolve(
                txn.ServiceID, txn.OperatorID, txn.ProviderID, slabMasterId, txn.DateTime);

            // ONE switch above everything, on Key Setting. Off until somebody turns it on, because
            // turning it on starts withholding real money from real people's commissions — and a
            // thing that takes money should never be inherited from a default nobody chose.
            bool masterOn = await _rules.IsMasterSwitchOn();
            if (!masterOn)
            {
                result.TDSEnabled = false;
                result.Note = "TDS is switched off for the whole platform (Key Setting → TDS). "
                            + "No rule is read and nothing is deducted while it is off.";
                return result;
            }

            // A per-party rule decides where one exists. Where none does, the commission tax
            // profile's own switch still decides, exactly as it did before rules existed — so this
            // changes nothing for an install that has written no rules.
            bool anyRule = await _context.TdsRules.AsNoTracking()
                .AnyAsync(r => !r.IsDeleted && r.IsActive);

            result.TDSEnabled = anyRule || treatment.TDSEnabled;
            if (!result.TDSEnabled)
            {
                // The ordinary case, and deliberately explicit: a zero that says why is a very
                // different thing from a zero nobody can account for.
                result.Note = "TDS is on for the platform, but no TDS rule is configured and the "
                            + "commission tax profile has TDS off for this service / provider / slab.";
                return result;
            }

            // Already deducted? A status-check sweep and a manual settle can both land on the same
            // transaction, and deducting twice takes real money from a real person.
            var existing = await _context.TransactionTDSDetails.AsNoTracking()
                .Where(d => d.TransactionMasterID == transactionMasterId && !d.IsDeleted)
                .ToListAsync();
            if (existing.Count > 0)
            {
                result.TotalDeducted = existing.Sum(d => d.TDSAmount);
                result.Note = "TDS was already deducted on this transaction.";
                result.Rows = existing.Select(d => new TdsDeductionRow
                {
                    PartyUserID = d.PartyUserID, Tier = d.Tier, SectionCode = d.SectionCode,
                    RatePercent = d.RatePercent, CommissionBase = d.CommissionBase,
                    TDSAmount = d.TDSAmount, NoPANRateApplied = d.NoPANRateApplied,
                }).ToList();
                return result;
            }

            // The three tiers that can earn a commission on one transaction. Only a POSITIVE
            // commission is income; a negative one is a surcharge the user PAID, and there is
            // nothing to deduct tax from.
            // StampedBase is the commission net of GST as the BAND that priced it says — the most
            // specific thing anyone configured, and the figure the transaction now carries. It is 0
            // for anything settled before the breakdown existed, and the profile split below is the
            // fallback for those.
            var tiers = new List<(string Tier, int UserId, decimal Commission, decimal StampedBase)>
            {
                (CommissionTier.Retailer, txn.UserID, txn.RetailerCommission, txn.RetailerBaseCharge),
                (CommissionTier.Distributor, txn.DistributerUserID, txn.DistributerCommission, txn.DistributerBaseCharge),
                (CommissionTier.SuperDistributor, txn.SuperDistributerUserID, txn.SuperDistributerCommission, txn.SuperDistributerBaseCharge),
            };

            var sections = await _taxConfig.GetTDSSections();
            string periodKey = txn.DateTime.ToString("yyyyMM");
            var toAdd = new List<TransactionTDSDetail>();

            foreach (var (tier, userId, commission, stampedBase) in tiers)
            {
                if (userId <= 0 || commission <= 0m) continue;

                // TDS is on INCOME, so the GST inside an inclusive commission comes out first.
                // Deducting on the gross figure would tax the tax.
                decimal basis = txn.ChargeGSTStampedOn != null && stampedBase > 0m
                    ? stampedBase
                    : _commissionTax.Split(commission, treatment, txn.Amount).NetCommission;
                if (basis <= 0m) continue;

                string sectionCode;
                decimal rate, tds;
                bool noPan;
                string chargeType = TdsChargeType.Percent;
                int ruleId = 0;
                string decidedBy;

                // The party's own role, which is what the USERTYPE scopes match on.
                int userTypeId = await _context.UserMasters.AsNoTracking()
                    .Where(u => u.UserMasterID == userId)
                    .Select(u => u.UserTypeID ?? 0)
                    .FirstOrDefaultAsync();

                var decision = await _rules.Resolve(userId, userTypeId, txn.ServiceID, txn.OperatorID, txn.DateTime);

                if (decision.RuleFound)
                {
                    var computed = await _rules.Compute(decision, userId, commission, basis);
                    if (computed.TDSAmount <= 0m)
                    {
                        // A rule matched and produced nothing. Say which rule and why, rather than
                        // leaving a silent zero that reads as "TDS is not configured".
                        result.Rows.Add(new TdsDeductionRow
                        {
                            PartyUserID = userId, PartyName = await PartyName(userId), Tier = tier,
                            SectionCode = computed.SectionCode, RatePercent = computed.RatePercent,
                            CommissionBase = computed.Base, TDSAmount = 0m,
                            NoPANRateApplied = computed.NoPANRateApplied,
                            Note = computed.SkipReason.Length > 0 ? computed.SkipReason : computed.DecidedBy,
                        });
                        continue;
                    }

                    sectionCode = computed.SectionCode;
                    rate = computed.RatePercent;
                    tds = computed.TDSAmount;
                    noPan = computed.NoPANRateApplied;
                    basis = computed.Base;
                    chargeType = computed.ChargeType;
                    ruleId = decision.TdsRuleID;
                    decidedBy = computed.DecidedBy;
                }
                else
                {
                    // No rule for this party. The profile path, unchanged.
                    if (!treatment.TDSEnabled) continue;

                    (sectionCode, rate, noPan) = await ResolveSection(userId, treatment, sections);
                    if (rate <= 0m) continue;

                    tds = Math.Round(basis * rate / 100m, 2, MidpointRounding.AwayFromZero);
                    if (tds <= 0m) continue;
                    decidedBy = $"No TDS rule matched; the {treatment.Scope.ToLowerInvariant()} commission tax "
                              + $"profile deducted {sectionCode} at {rate:0.####}%";
                }

                // Take it. Until now TDS was computed and recorded but never actually came out of
                // anybody's wallet, so the platform showed a liability it had not collected. The
                // commission was credited at initiation, so the balance normally covers this.
                bool withheld = await Withhold(txn, userId, tier, tds, sectionCode);

                toAdd.Add(new TransactionTDSDetail
                {
                    TransactionMasterID = transactionMasterId,
                    PartyUserID = userId,
                    Tier = tier,
                    SectionCode = sectionCode,
                    RatePercent = rate,
                    CommissionBase = basis,
                    TDSAmount = tds,
                    NoPANRateApplied = noPan,
                    PeriodKey = periodKey,
                    TdsRuleID = ruleId,
                    ChargeType = chargeType,
                    Withheld = withheld,
                    DecidedBy = Trim(decidedBy, 200),
                    DateTime = DateTime.Now,
                    UpdatedateTime = DateTime.Now,
                    IsDeleted = false,
                });

                string name = await PartyName(userId);
                result.Rows.Add(new TdsDeductionRow
                {
                    PartyUserID = userId, PartyName = name, Tier = tier,
                    SectionCode = sectionCode, RatePercent = rate,
                    CommissionBase = basis, TDSAmount = tds, NoPANRateApplied = noPan,
                    ChargeType = chargeType, Withheld = withheld, Note = decidedBy,
                });
            }

            if (toAdd.Count == 0)
            {
                result.Note = "TDS is on, but no tier had a positive commission to deduct from.";
                return result;
            }

            _context.TransactionTDSDetails.AddRange(toAdd);

            // Mirror the totals onto the transaction, because that is where every existing report
            // reads them from. The detail — section, rate, base — stays in the table above, which
            // is what a TDS certificate is actually made of.
            var tracked = await _context.TransactionMasters
                .FirstOrDefaultAsync(t => t.TransactionMasterID == transactionMasterId);
            if (tracked != null)
            {
                tracked.RetailerTDS = toAdd.FirstOrDefault(d => d.Tier == CommissionTier.Retailer)?.TDSAmount ?? 0m;
                tracked.DistributerTDS = toAdd.FirstOrDefault(d => d.Tier == CommissionTier.Distributor)?.TDSAmount ?? 0m;
                tracked.SuperDistributerTDS = toAdd.FirstOrDefault(d => d.Tier == CommissionTier.SuperDistributor)?.TDSAmount ?? 0m;
                tracked.UpdatedateTime = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            result.TotalDeducted = Math.Round(toAdd.Sum(d => d.TDSAmount), 2);
            result.Note = $"Deducted from {toAdd.Count} tier(s) under {string.Join(", ", toAdd.Select(d => d.SectionCode).Distinct())}.";
            return result;
        }

        /// <summary>
        /// Take the deduction out of the party's wallet, on the same wallet the commission was
        /// credited to and through the same posting service — so it is locked, it writes a ledger
        /// row, and it shows up in the user's own statement with a line saying what it was.
        ///
        /// Returns false rather than throwing when it cannot be taken. The money has already moved
        /// by the time this runs; a deduction that is recorded but not collected is a visible,
        /// reportable state, and the monthly report keeps it apart so nobody deposits money the
        /// platform never actually holds.
        /// </summary>
        private async Task<bool> Withhold(TransactionMaster txn, int userId, string tier, decimal tds, string sectionCode)
        {
            try
            {
                if (tds <= 0m) return false;

                var posting = new Requests.AccountProcessing.AccountPostingRequest
                {
                    UserID = userId,
                    WalletID = txn.WalletID,
                    AmountWithCommission = tds,
                    Amount = 0,
                    ConsumerNumber = txn.ConsumerNumber ?? string.Empty,
                    CommissionCreditDebit = CommissionCreditDebitType.Debit,
                    DOCType = DOCTypeConstants.Recharge,
                    AssignedUserID = txn.UserID,
                    ServiceID = txn.ServiceID,
                    OperatorID = txn.OperatorID,
                    TransactionDescription = $"TDS {sectionCode} withheld on {tier.ToLowerInvariant()} commission, txn #{txn.TransactionMasterID}",
                    ReferenceID = txn.TransactionMasterID,
                    TDS = tds,
                };

                int rows = await _account.DebitBalance(posting);
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return false;
            }
        }

        private static string Trim(string? s, int max)
            => string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= max ? s : s.Substring(0, max));

        /// <summary>
        /// Which section and rate apply to this party.
        ///
        /// The profile may pin both. Where it does not, the section comes from the party's
        /// RELATIONSHIP on this service — the Bharti Cellular distinction the tax engine already
        /// models: a principal-to-principal distributor's discount is not commission and 194H does
        /// not reach it, while a true agent's is. Without a PAN the section's higher no-PAN rate
        /// applies, which is the one case where the rate roughly doubles.
        /// </summary>
        private async Task<(string Section, decimal Rate, bool NoPan)> ResolveSection(
            int userId, CommissionTaxTreatment treatment, List<TDSSectionMaster> sections)
        {
            string code = (treatment.TDSSectionCode ?? string.Empty).Trim();
            if (code.Length == 0) code = "194H";   // commission or brokerage, the default for this trade

            var section = sections.FirstOrDefault(s =>
                s.IsActive && !s.IsDeleted &&
                string.Equals(s.SectionCode, code, StringComparison.OrdinalIgnoreCase));

            decimal rate = treatment.TDSRatePercent > 0m
                ? treatment.TDSRatePercent
                : section?.Rate ?? 0m;

            bool noPan = false;
            var profile = await _taxConfig.GetPartyTaxProfile(userId);
            string pan = (profile?.PAN ?? string.Empty).Trim();
            if (pan.Length == 0)
            {
                pan = await _context.UserMasters.AsNoTracking()
                    .Where(u => u.UserMasterID == userId)
                    .Select(u => u.PANNo ?? string.Empty).FirstOrDefaultAsync() ?? string.Empty;
                pan = pan.Trim();
            }

            if (pan.Length != 10 && section != null && section.NoPANRate > 0m)
            {
                rate = section.NoPANRate;
                noPan = true;
            }

            return (section?.SectionCode ?? code, rate, noPan);
        }

        private async Task<string> PartyName(int userId)
        {
            var u = await _context.UserMasters.AsNoTracking()
                .Where(x => x.UserMasterID == userId)
                .Select(x => new { x.FirmName, x.PersonName }).FirstOrDefaultAsync();
            if (u == null) return $"#{userId}";
            return !string.IsNullOrWhiteSpace(u.FirmName) ? u.FirmName : (u.PersonName ?? $"#{userId}");
        }
    }
}
