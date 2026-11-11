using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.AccountProcessing;
using FintechApp.Requests.Tds;
using FintechApp.Responses.Tds;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="ITdsAdminService" />
    public class TdsAdminService : ITdsAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITdsRuleResolver _rules;
        private readonly ICommissionTaxResolver _commissionTax;
        private readonly ITaxConfigurationRepository _taxConfig;
        private readonly IAccountProcessingService _account;
        private readonly Logger _logger = new();

        public TdsAdminService(
            ApplicationDbContext context,
            ITdsRuleResolver rules,
            ICommissionTaxResolver commissionTax,
            ITaxConfigurationRepository taxConfig,
            IAccountProcessingService account)
        {
            _context = context;
            _rules = rules;
            _commissionTax = commissionTax;
            _taxConfig = taxConfig;
            _account = account;
        }

        // ── The rules ───────────────────────────────────────────────────────────────────────

        public async Task<TdsSettingView> GetRules()
        {
            var view = new TdsSettingView { MasterSwitchOn = await _rules.IsMasterSwitchOn() };

            var rules = await _context.TdsRules.AsNoTracking()
                .Where(r => !r.IsDeleted)
                .ToListAsync();

            var users = await Names();
            var types = await _context.UserTypeMasters.AsNoTracking()
                .ToDictionaryAsync(t => t.UserTypeID, t => t.UserType ?? string.Empty);
            var services = await _context.ServiceMasters.AsNoTracking()
                .ToDictionaryAsync(s => s.ServiceID, s => s.Name ?? string.Empty);
            var operators = await _context.OperatorMasters.AsNoTracking()
                .ToDictionaryAsync(o => o.OperatorID, o => o.OperatorName ?? string.Empty);

            view.Rules = rules
                .OrderByDescending(r => r.Priority > 0 ? r.Priority : TdsScope.Rank(r.Scope))
                .ThenByDescending(r => r.TdsRuleID)
                .Select(r =>
                {
                    string userName = users.TryGetValue(r.UserID, out var u) ? u : string.Empty;
                    string typeName = types.TryGetValue(r.UserTypeID, out var t) ? t : string.Empty;
                    string serviceName = services.TryGetValue(r.ServiceID, out var s) ? s : string.Empty;
                    string operatorName = operators.TryGetValue(r.OperatorID, out var o) ? o : string.Empty;

                    return new TdsRuleRow
                    {
                        TdsRuleID = r.TdsRuleID,
                        Scope = r.Scope,
                        ScopeLabel = ScopeLabel(r.Scope, userName, typeName, serviceName, operatorName),
                        UserID = r.UserID, UserName = userName,
                        UserTypeID = r.UserTypeID, UserTypeName = typeName,
                        ServiceID = r.ServiceID, ServiceName = serviceName,
                        OperatorID = r.OperatorID, OperatorName = operatorName,
                        ChargeType = TdsChargeType.Normalize(r.ChargeType),
                        ChargeValue = r.ChargeValue,
                        ApplyOn = TdsApplyOn.Normalize(r.ApplyOn),
                        MinCommission = r.MinCommission,
                        MaxTDSPerTransaction = r.MaxTDSPerTransaction,
                        SectionCode = string.IsNullOrWhiteSpace(r.SectionCode) ? "194H" : r.SectionCode.Trim(),
                        ApplyNoPANRate = r.ApplyNoPANRate,
                        EffectiveFrom = r.EffectiveFrom,
                        EffectiveTo = r.EffectiveTo,
                        Priority = r.Priority,
                        Remark = r.Remark ?? string.Empty,
                        IsActive = r.IsActive,
                        Summary = Summary(r, userName, typeName, serviceName),
                    };
                }).ToList();

            var sections = await _taxConfig.GetTDSSections();
            view.Sections = sections.Where(s => s.IsActive && !s.IsDeleted)
                .Select(s => new TdsSectionOption
                {
                    SectionCode = s.SectionCode, Rate = s.Rate,
                    NoPANRate = s.NoPANRate, ThresholdAnnual = s.ThresholdAnnual,
                }).ToList();

            view.Note = view.MasterSwitchOn
                ? view.Rules.Count == 0
                    ? "TDS is on for the platform but no rule is configured, so the commission tax profile decides — which has TDS off everywhere by default."
                    : "TDS is on. The most specific rule that matches a party wins."
                : "TDS is OFF for the whole platform (Key Setting → TDS). Rules can be written now and take effect the moment it is switched on; nothing is deducted until then.";

            return view;
        }

        public async Task<(int RuleID, string Failure)> SaveRule(SaveTdsRuleRequest request)
        {
            string scope = (request.Scope ?? string.Empty).Trim().ToUpperInvariant();
            if (!TdsScope.All.Contains(scope))
                return (0, "Pick a scope: this user on this service, this user, a whole role on a service, a whole role, a service, or the house default.");

            if (TdsScope.NeedsUser(scope) && request.UserID <= 0)
                return (0, "That scope is about one person, so pick the user it applies to.");
            if (TdsScope.NeedsUserType(scope) && request.UserTypeID <= 0)
                return (0, "That scope is about a role, so pick the role it applies to.");
            if (TdsScope.NeedsService(scope) && request.ServiceID <= 0)
                return (0, "That scope is about one service, so pick the service it applies to.");

            string chargeType = TdsChargeType.Normalize(request.ChargeType);
            if (request.ChargeValue < 0m)
                return (0, "The TDS figure cannot be negative.");
            if (chargeType == TdsChargeType.Percent && request.ChargeValue > 100m)
                return (0, "A TDS percentage over 100 is a typo. Note that 0.01 means 0.01%, not 1%.");
            if (request.MinCommission < 0m || request.MaxTDSPerTransaction < 0m)
                return (0, "The minimum commission and the per-transaction cap cannot be negative.");

            DateTime from = (request.EffectiveFrom ?? new DateTime(2000, 1, 1)).Date;
            DateTime? to = request.EffectiveTo?.Date;
            if (to != null && to < from)
                return (0, "The rule cannot end before it starts.");

            string section = (request.SectionCode ?? string.Empty).Trim();
            if (section.Length > 0)
            {
                var sections = await _taxConfig.GetTDSSections();
                if (!sections.Any(s => s.IsActive && !s.IsDeleted
                                    && string.Equals(s.SectionCode, section, StringComparison.OrdinalIgnoreCase)))
                    return (0, $"There is no active TDS section '{section}'. Leave it blank to use 194H.");
            }

            // The same scope, for the same party/role/service/operator, over the same dates, twice
            // would make which one wins depend on the id. Refuse the duplicate instead.
            var clash = await _context.TdsRules
                .Where(r => !r.IsDeleted && r.TdsRuleID != request.TdsRuleID
                         && r.Scope == scope
                         && r.UserID == request.UserID
                         && r.UserTypeID == request.UserTypeID
                         && r.ServiceID == request.ServiceID
                         && r.OperatorID == request.OperatorID
                         && r.EffectiveFrom == from)
                .FirstOrDefaultAsync();
            if (clash != null)
                return (0, $"Rule #{clash.TdsRuleID} already covers exactly this, from the same date. Edit that one instead.");

            var entity = request.TdsRuleID > 0
                ? await _context.TdsRules.FirstOrDefaultAsync(r => r.TdsRuleID == request.TdsRuleID && !r.IsDeleted)
                : new TdsRule { DateTime = DateTime.Now, CreatedByUserID = request.CreatedByUserID };

            if (entity == null) return (0, "That rule no longer exists.");

            entity.Scope = scope;
            entity.UserID = TdsScope.NeedsUser(scope) ? request.UserID : 0;
            entity.UserTypeID = TdsScope.NeedsUserType(scope) ? request.UserTypeID : 0;
            entity.ServiceID = TdsScope.NeedsService(scope) ? request.ServiceID : 0;
            entity.OperatorID = request.OperatorID;
            entity.ChargeType = chargeType;
            entity.ChargeValue = request.ChargeValue;
            entity.ApplyOn = TdsApplyOn.Normalize(request.ApplyOn);
            entity.MinCommission = request.MinCommission;
            entity.MaxTDSPerTransaction = request.MaxTDSPerTransaction;
            entity.SectionCode = section;
            entity.ApplyNoPANRate = request.ApplyNoPANRate;
            entity.EffectiveFrom = from;
            entity.EffectiveTo = to;
            entity.Priority = request.Priority;
            entity.Remark = (request.Remark ?? string.Empty).Trim();
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.Now;

            if (request.TdsRuleID <= 0) _context.TdsRules.Add(entity);
            await _context.SaveChangesAsync();

            return (entity.TdsRuleID, string.Empty);
        }

        public async Task<string> DeleteRule(int ruleId)
        {
            var entity = await _context.TdsRules.FirstOrDefaultAsync(r => r.TdsRuleID == ruleId && !r.IsDeleted);
            if (entity == null) return "That rule no longer exists.";

            // Soft delete, like everything else here: a deduction already taken under this rule
            // points at it, and a report that cannot name the rule it used is a report nobody can
            // defend.
            entity.IsDeleted = true;
            entity.UpdatedateTime = DateTime.Now;
            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        // ── Try a figure ────────────────────────────────────────────────────────────────────

        public async Task<(TdsPreviewResponse? Preview, string Failure)> Preview(TdsPreviewRequest request)
        {
            if (request.UserID <= 0) return (null, "Pick the user who would earn the commission — the rules are about people.");
            if (request.Commission <= 0m) return (null, "Enter a commission greater than 0. A negative figure is a charge the user paid, which carries no TDS.");

            DateTime asOf = (request.AsOf ?? DateTime.Now).Date;

            int userTypeId = await _context.UserMasters.AsNoTracking()
                .Where(u => u.UserMasterID == request.UserID)
                .Select(u => u.UserTypeID ?? 0)
                .FirstOrDefaultAsync();

            var response = new TdsPreviewResponse
            {
                MasterSwitchOn = await _rules.IsMasterSwitchOn(),
                CommissionAsQuoted = R2(request.Commission),
            };

            // The GST inside the commission. Given explicitly, or worked out the way a settlement
            // does — through the commission tax profile — so the preview and the real thing agree.
            decimal gst = request.CommissionGST;
            if (gst <= 0m)
            {
                var treatment = await _commissionTax.Resolve(request.ServiceID, request.OperatorID, 0, 0, asOf);
                var split = _commissionTax.Split(request.Commission, treatment, request.Commission);
                gst = split.GST;
                response.Steps.Add($"Commission {Money(request.Commission)} is {treatment.TaxMode.ToLowerInvariant()} of GST "
                                 + $"at {treatment.GSTRatePercent:0.##}% ({treatment.Scope.ToLowerInvariant()} profile), so {Money(gst)} of it is GST.");
            }
            else
            {
                response.Steps.Add($"GST inside the commission was given as {Money(gst)}.");
            }

            decimal net = R2(request.Commission - Math.Abs(gst));
            response.GSTInCommission = R2(Math.Abs(gst));
            response.Base = net;

            var decision = await _rules.Resolve(request.UserID, userTypeId, request.ServiceID, request.OperatorID, asOf);
            response.RuleFound = decision.RuleFound;
            response.TdsRuleID = decision.TdsRuleID;
            response.Scope = decision.Scope;
            response.DecidedBy = decision.DecidedBy;

            if (!decision.RuleFound)
            {
                response.Steps.Add("No TDS rule matches this user on this service, so nothing would be deducted by the rules. "
                                 + "The commission tax profile's own TDS switch would decide instead.");
                response.Warning = "No rule matched. Add one for this user, this role, or this service.";
                response.NetToParty = R2(request.Commission);
                return (response, string.Empty);
            }

            var computed = await _rules.Compute(decision, request.UserID, request.Commission, net);

            response.ChargeType = computed.ChargeType;
            response.RatePercent = computed.RatePercent;
            response.TDSAmount = computed.TDSAmount;
            response.Base = computed.Base;
            response.SectionCode = computed.SectionCode;
            response.NoPANRateApplied = computed.NoPANRateApplied;
            response.NetToParty = R2(request.Commission - computed.TDSAmount);

            response.Steps.Add($"{decision.DecidedBy}.");
            response.Steps.Add(decision.ApplyOn == TdsApplyOn.CommissionGross
                ? $"The rule deducts on the commission as quoted, so the base is {Money(computed.Base)}."
                : $"TDS is a tax on income, so the base is the commission net of GST: {Money(computed.Base)}.");

            if (computed.ChargeType == TdsChargeType.Fix)
                response.Steps.Add($"A flat {Money(decision.ChargeValue)} per transaction, so TDS is {Money(computed.TDSAmount)}.");
            else
                response.Steps.Add($"{computed.RatePercent:0.####}% of {Money(computed.Base)} is {Money(computed.TDSAmount)}"
                                 + (computed.NoPANRateApplied ? " — at the section's HIGHER no-PAN rate, because this party has no PAN on record." : "."));

            if (decision.MaxTDSPerTransaction > 0m && computed.TDSAmount >= decision.MaxTDSPerTransaction)
                response.Steps.Add($"The rule caps one transaction at {Money(decision.MaxTDSPerTransaction)}.");

            response.Steps.Add($"The party is paid {Money(response.NetToParty)} and {Money(computed.TDSAmount)} is withheld under {computed.SectionCode}.");

            if (computed.SkipReason.Length > 0) response.Warning = computed.SkipReason;
            else if (!response.MasterSwitchOn)
                response.Warning = "TDS is OFF for the whole platform, so nothing is actually being deducted yet. "
                                 + "Turn it on under Key Setting when the rules are ready.";

            return (response, string.Empty);
        }

        // ── The monthly list ────────────────────────────────────────────────────────────────

        public async Task<TdsMonthlyReport> MonthlyReport(TdsMonthlyReportRequest request)
        {
            string period = (request.PeriodKey ?? string.Empty).Trim();
            if (period.Length != 6 || !int.TryParse(period, out _)) period = DateTime.Now.ToString("yyyyMM");

            var report = new TdsMonthlyReport
            {
                PeriodKey = period,
                PeriodLabel = PeriodLabel(period),
                MasterSwitchOn = await _rules.IsMasterSwitchOn(),
            };

            var details = await _context.TransactionTDSDetails.AsNoTracking()
                .Where(d => !d.IsDeleted && d.PeriodKey == period
                         && (request.PartyUserID <= 0 || d.PartyUserID == request.PartyUserID))
                .ToListAsync();

            var settlements = await _context.TdsSettlements.AsNoTracking()
                .Where(s => !s.IsDeleted && s.PeriodKey == period)
                .ToDictionaryAsync(s => s.PartyUserID, s => s);

            var users = await Names();
            var types = await _context.UserTypeMasters.AsNoTracking()
                .ToDictionaryAsync(t => t.UserTypeID, t => t.UserType ?? string.Empty);

            var profiles = await _context.PartyTaxProfiles.AsNoTracking()
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            var codes = await _context.UserMasters.AsNoTracking()
                .Select(u => new { u.UserMasterID, u.AliasName, u.PANNo, u.UserTypeID })
                .ToListAsync();
            var codeById = codes.ToDictionary(u => u.UserMasterID, u => u);

            foreach (var group in details.GroupBy(d => d.PartyUserID))
            {
                codeById.TryGetValue(group.Key, out var u);
                string pan = (profiles.FirstOrDefault(p => p.UserID == group.Key)?.PAN ?? string.Empty).Trim();
                if (pan.Length == 0) pan = (u?.PANNo ?? string.Empty).Trim();

                settlements.TryGetValue(group.Key, out var settled);

                var row = new TdsMonthlyRow
                {
                    PartyUserID = group.Key,
                    PartyName = users.TryGetValue(group.Key, out var n) ? n : $"user {group.Key}",
                    PartyCode = u?.AliasName ?? string.Empty,
                    UserTypeName = u != null && types.TryGetValue(u.UserTypeID ?? 0, out var t) ? t : string.Empty,
                    PAN = pan,
                    PANMissing = pan.Length != 10,
                    DeductionCount = group.Count(),
                    CommissionBase = R2(group.Sum(d => d.CommissionBase)),
                    TDSAmount = R2(group.Sum(d => d.TDSAmount)),
                    NotWithheld = R2(group.Where(d => !d.Withheld).Sum(d => d.TDSAmount)),
                    Sections = group.Select(d => d.SectionCode).Where(s => !string.IsNullOrWhiteSpace(s))
                                    .Distinct().OrderBy(s => s).ToList(),
                    SettlementMode = settled?.Mode ?? string.Empty,
                    SettlementReference = settled?.Reference ?? string.Empty,
                    SettledOn = settled?.SettledOn,
                    SettledAmount = settled?.TDSAmount ?? 0m,
                };
                row.Pending = settled == null;
                report.Rows.Add(row);
            }

            if (request.PendingOnly) report.Rows = report.Rows.Where(r => r.Pending).ToList();

            report.Rows = report.Rows.OrderByDescending(r => r.TDSAmount).ToList();
            report.PartyCount = report.Rows.Count;
            report.TotalTDS = R2(report.Rows.Sum(r => r.TDSAmount));
            report.TotalNotWithheld = R2(report.Rows.Sum(r => r.NotWithheld));
            report.TotalDeposited = R2(report.Rows.Where(r => r.SettlementMode == TdsSettlementMode.Deposited).Sum(r => r.SettledAmount));
            report.TotalWalletSettled = R2(report.Rows.Where(r => r.SettlementMode == TdsSettlementMode.WalletSettled).Sum(r => r.SettledAmount));
            report.TotalPending = R2(report.Rows.Where(r => r.Pending).Sum(r => r.TDSAmount));

            report.Note = report.Rows.Count == 0
                ? report.MasterSwitchOn
                    ? $"No TDS was deducted in {report.PeriodLabel}."
                    : $"TDS is off for the platform, so nothing was deducted in {report.PeriodLabel}."
                : $"{report.PartyCount} part{(report.PartyCount == 1 ? "y" : "ies")}, {Money(report.TotalTDS)} withheld, {Money(report.TotalPending)} still to be dealt with.";

            return report;
        }

        public async Task<TdsPartyDetailResponse> PartyDetail(string periodKey, int partyUserID)
        {
            string period = (periodKey ?? string.Empty).Trim();
            var response = new TdsPartyDetailResponse { PeriodKey = period, PartyUserID = partyUserID };
            if (period.Length != 6 || partyUserID <= 0) return response;

            var rows = await _context.TransactionTDSDetails.AsNoTracking()
                .Where(d => !d.IsDeleted && d.PeriodKey == period && d.PartyUserID == partyUserID)
                .ToListAsync();

            var txnIds = rows.Select(r => r.TransactionMasterID).Distinct().ToList();
            var txns = await _context.TransactionMasters.AsNoTracking()
                .Where(t => txnIds.Contains(t.TransactionMasterID))
                .Select(t => new { t.TransactionMasterID, t.DateTime, t.ServiceID })
                .ToListAsync();
            var services = await _context.ServiceMasters.AsNoTracking()
                .ToDictionaryAsync(s => s.ServiceID, s => s.Name ?? string.Empty);

            var users = await Names();
            response.PartyName = users.TryGetValue(partyUserID, out var n) ? n : $"user {partyUserID}";
            string pan = (await _context.PartyTaxProfiles.AsNoTracking()
                .Where(p => !p.IsDeleted && p.UserID == partyUserID)
                .Select(p => p.PAN).FirstOrDefaultAsync() ?? string.Empty).Trim();
            if (pan.Length == 0)
                pan = (await _context.UserMasters.AsNoTracking()
                    .Where(x => x.UserMasterID == partyUserID)
                    .Select(x => x.PANNo ?? string.Empty).FirstOrDefaultAsync() ?? string.Empty).Trim();
            response.PAN = pan;

            response.Rows = rows.Select(d =>
            {
                var t = txns.FirstOrDefault(x => x.TransactionMasterID == d.TransactionMasterID);
                return new TdsDeductionDetailRow
                {
                    TransactionMasterID = d.TransactionMasterID,
                    DateTime = t?.DateTime ?? d.DateTime,
                    ServiceName = t != null && services.TryGetValue(t.ServiceID, out var s) ? s : string.Empty,
                    Tier = d.Tier,
                    CommissionBase = d.CommissionBase,
                    ChargeType = string.IsNullOrWhiteSpace(d.ChargeType) ? TdsChargeType.Percent : d.ChargeType,
                    RatePercent = d.RatePercent,
                    TDSAmount = d.TDSAmount,
                    SectionCode = d.SectionCode,
                    NoPANRateApplied = d.NoPANRateApplied,
                    Withheld = d.Withheld,
                    DecidedBy = d.DecidedBy ?? string.Empty,
                };
            }).OrderBy(r => r.DateTime).ToList();

            response.TotalTDS = R2(response.Rows.Sum(r => r.TDSAmount));
            return response;
        }

        // ── The two things that can be done with a month ────────────────────────────────────

        public async Task<string> MarkDeposited(TdsDepositRequest request)
        {
            string period = (request.PeriodKey ?? string.Empty).Trim();
            if (period.Length != 6) return "Pick the month this deposit is for.";
            if (request.PartyUserID <= 0) return "Pick the party this deposit is for.";
            if (string.IsNullOrWhiteSpace(request.Reference))
                return "Enter the challan / BSR reference. A TDS certificate is issued against it, so a deposit without one cannot be proved.";

            var (total, count, failure) = await PeriodTotal(period, request.PartyUserID);
            if (failure.Length > 0) return failure;

            var existing = await _context.TdsSettlements
                .FirstOrDefaultAsync(s => !s.IsDeleted && s.PeriodKey == period && s.PartyUserID == request.PartyUserID);
            if (existing != null)
                return existing.Mode == TdsSettlementMode.Deposited
                    ? $"This month was already recorded as deposited on {existing.SettledOn:dd MMM yyyy} against {existing.Reference}."
                    : "This month was already settled back into the party's wallet, so it cannot also be deposited.";

            _context.TdsSettlements.Add(new TdsSettlement
            {
                PeriodKey = period,
                PartyUserID = request.PartyUserID,
                Mode = TdsSettlementMode.Deposited,
                TDSAmount = total,
                DeductionCount = count,
                Reference = request.Reference.Trim(),
                ReferenceDate = request.ReferenceDate?.Date,
                Remark = (request.Remark ?? string.Empty).Trim(),
                SettledOn = DateTime.Now,
                SettledByUserID = request.SettledByUserID,
                DateTime = DateTime.Now,
                UpdatedateTime = DateTime.Now,
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Two desks pressed it at once. The unique index held; the other one is the record.
                return "Another user recorded this month for this party a moment ago.";
            }

            return StatusType.Ok;
        }

        public async Task<(decimal Credited, string Failure)> SettleToWallet(TdsWalletSettleRequest request)
        {
            string period = (request.PeriodKey ?? string.Empty).Trim();
            if (period.Length != 6) return (0m, "Pick the month to settle.");
            if (request.PartyUserID <= 0) return (0m, "Pick the party to settle.");
            if (string.IsNullOrWhiteSpace(request.Remark))
                return (0m, "Say why this is going back to the party rather than to the government. This moves real money, and the reason is what makes it answerable later.");

            var (total, count, failure) = await PeriodTotal(period, request.PartyUserID);
            if (failure.Length > 0) return (0m, failure);

            var existing = await _context.TdsSettlements
                .FirstOrDefaultAsync(s => !s.IsDeleted && s.PeriodKey == period && s.PartyUserID == request.PartyUserID);
            if (existing != null)
                return (0m, existing.Mode == TdsSettlementMode.WalletSettled
                    ? $"This month was already settled into the party's wallet on {existing.SettledOn:dd MMM yyyy}."
                    : $"This month was already recorded as deposited against {existing.Reference}, so it cannot be returned as well.");

            // Only what was actually COLLECTED can be given back. Crediting a wallet for a
            // deduction that never came out of it would hand the party money twice.
            var withheld = await _context.TransactionTDSDetails.AsNoTracking()
                .Where(d => !d.IsDeleted && d.PeriodKey == period && d.PartyUserID == request.PartyUserID && d.Withheld)
                .Select(d => new { d.TDSAmount, d.TransactionMasterID })
                .ToListAsync();

            decimal collected = R2(withheld.Sum(d => d.TDSAmount));
            if (collected <= 0m)
                return (0m, "Nothing was actually withheld from this party in that month, so there is nothing to give back.");

            // Give it back into the wallet it came out of. The deduction was taken from the wallet
            // the transaction ran on, and an install can have several, so the money goes back the
            // way it came rather than into a wallet somebody picked.
            var txnIds = withheld.Select(d => d.TransactionMasterID).Distinct().ToList();
            var walletByTxn = await _context.TransactionMasters.AsNoTracking()
                .Where(t => txnIds.Contains(t.TransactionMasterID))
                .Select(t => new { t.TransactionMasterID, t.WalletID })
                .ToDictionaryAsync(t => t.TransactionMasterID, t => t.WalletID);

            var shares = new Dictionary<int, decimal>();
            foreach (var d in withheld)
            {
                int walletId = request.WalletID > 0
                    ? request.WalletID
                    : walletByTxn.TryGetValue(d.TransactionMasterID, out var w) && w > 0 ? w : 0;
                if (walletId <= 0) continue;
                shares[walletId] = shares.TryGetValue(walletId, out var sum) ? sum + d.TDSAmount : d.TDSAmount;
            }

            if (shares.Count == 0)
                return (0m, "Could not tell which wallet the TDS came out of. Pick a wallet explicitly and try again.");

            decimal credited = 0m;
            foreach (var (walletId, amount) in shares)
            {
                decimal share = R2(amount);
                if (share <= 0m) continue;

                int rows = await _account.CreditBalance(new AccountPostingRequest
                {
                    UserID = request.PartyUserID,
                    WalletID = walletId,
                    AmountWithCommission = share,
                    Amount = 0,
                    CommissionCreditDebit = CommissionCreditDebitType.Credit,
                    DOCType = DOCTypeConstants.Recharge,
                    AssignedUserID = request.PartyUserID,
                    TransactionDescription = $"TDS of {PeriodLabel(period)} returned to wallet — {request.Remark.Trim()}",
                });

                if (rows > 0) credited += share;
            }

            if (credited <= 0m)
                return (0m, "The wallet credit did not go through, so nothing was settled. Nothing has been recorded.");

            _context.TdsSettlements.Add(new TdsSettlement
            {
                PeriodKey = period,
                PartyUserID = request.PartyUserID,
                Mode = TdsSettlementMode.WalletSettled,
                TDSAmount = credited,
                DeductionCount = count,
                Reference = string.Empty,
                WalletID = request.WalletID,
                Remark = request.Remark.Trim(),
                SettledOn = DateTime.Now,
                SettledByUserID = request.SettledByUserID,
                DateTime = DateTime.Now,
                UpdatedateTime = DateTime.Now,
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // The money HAS moved. Record and report rather than pretending it did not.
                _logger.Write(ex);
                return (credited, $"{Money(credited)} was credited to the wallet, but the settlement record could not be written. "
                                + "Do not run this again — check the party's wallet statement first.");
            }

            return (credited, string.Empty);
        }

        // ── Plumbing ────────────────────────────────────────────────────────────────────────

        private async Task<(decimal Total, int Count, string Failure)> PeriodTotal(string period, int partyUserID)
        {
            var rows = await _context.TransactionTDSDetails.AsNoTracking()
                .Where(d => !d.IsDeleted && d.PeriodKey == period && d.PartyUserID == partyUserID)
                .Select(d => d.TDSAmount)
                .ToListAsync();

            if (rows.Count == 0)
                return (0m, 0, $"No TDS was deducted from this party in {PeriodLabel(period)}.");

            return (R2(rows.Sum()), rows.Count, string.Empty);
        }

        private async Task<Dictionary<int, string>> Names()
        {
            var users = await _context.UserMasters.AsNoTracking()
                .Select(u => new { u.UserMasterID, u.PersonName, u.FirmName })
                .ToListAsync();

            return users.ToDictionary(
                u => u.UserMasterID,
                u => !string.IsNullOrWhiteSpace(u.FirmName) ? u.FirmName!
                   : !string.IsNullOrWhiteSpace(u.PersonName) ? u.PersonName!
                   : $"user {u.UserMasterID}");
        }

        private static string ScopeLabel(string scope, string user, string role, string service, string op)
        {
            string suffix = string.IsNullOrWhiteSpace(op) ? string.Empty : $" ({op} only)";
            return (scope ?? string.Empty).ToUpperInvariant() switch
            {
                TdsScope.UserService => $"{user} on {service}{suffix}",
                TdsScope.User => $"{user}, every service{suffix}",
                TdsScope.UserTypeService => $"every {role} on {service}{suffix}",
                TdsScope.UserType => $"every {role}, every service{suffix}",
                TdsScope.Service => $"everyone on {service}{suffix}",
                TdsScope.Global => $"the house default — everyone, every service{suffix}",
                _ => scope ?? string.Empty,
            };
        }

        private static string Summary(TdsRule r, string user, string role, string service)
        {
            string figure = TdsChargeType.Normalize(r.ChargeType) == TdsChargeType.Fix
                ? $"a flat {Money(r.ChargeValue)} per transaction"
                : $"{r.ChargeValue:0.####}% of the commission"
                  + (TdsApplyOn.Normalize(r.ApplyOn) == TdsApplyOn.CommissionGross ? " as quoted" : " net of GST");

            var parts = new List<string> { $"Deducts {figure}" };
            if (r.MinCommission > 0m) parts.Add($"only above a commission of {Money(r.MinCommission)}");
            if (r.MaxTDSPerTransaction > 0m) parts.Add($"capped at {Money(r.MaxTDSPerTransaction)} per transaction");
            if (!r.ApplyNoPANRate) parts.Add("the higher no-PAN rate is NOT applied");
            if (!r.IsActive) parts.Add("currently inactive");

            return string.Join(", ", parts) + ".";
        }

        private static string PeriodLabel(string period)
            => period.Length == 6 && int.TryParse(period.Substring(0, 4), out var y) && int.TryParse(period.Substring(4, 2), out var m) && m is >= 1 and <= 12
                ? new DateTime(y, m, 1).ToString("MMMM yyyy")
                : period;

        private static string Money(decimal v) => $"Rs {v:0.00}";
        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    }
}
