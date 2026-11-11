using System.Text.RegularExpressions;
using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IBankLineClassifier" />
    ///
    /// Reconciliation answers "did this happen?" by pairing a bank line with a voucher. It cannot
    /// answer "what was it?", and that is the half an operator actually writes down: stock bought
    /// from a provider, a retailer's deposit, rent, money moved between our own accounts.
    ///
    /// The only evidence is the narration the bank wrote, so the rules read that. Everything they
    /// decide is stored on the line — a category re-derived on every screen open would change
    /// under the reader the first time somebody edits a rule.
    public class BankLineClassifier : IBankLineClassifier
    {
        private readonly ApplicationDbContext _context;
        private readonly Logger _logger = new();

        public BankLineClassifier(ApplicationDbContext context) => _context = context;

        /// <summary>A UTR / RRN as it appears inside a narration.</summary>
        private static readonly Regex RefInText =
            new(@"\b([A-Z]{3,6}\d{6,}|\d{10,})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public async Task<int> Classify(List<BankStatementLine> lines, int bankAccountId)
        {
            if (lines.Count == 0) return 0;

            var rules = await _context.BankLineCategoryRules.AsNoTracking()
                .Where(r => !r.IsDeleted && r.IsActive && (r.BankAccountID == 0 || r.BankAccountID == bankAccountId))
                .ToListAsync();

            // Most specific first: a rule pinned to this bank beats a general one, then an explicit
            // priority, then the longer pattern — "NEFT DR-JAIJUI" should win over "NEFT".
            var ordered = rules
                .OrderByDescending(r => r.Priority > 0 ? r.Priority : DerivedPriority(r))
                .ThenByDescending(r => (r.Pattern ?? string.Empty).Length)
                .ThenBy(r => r.BankLineCategoryRuleID)
                .ToList();

            // The deposits we know about, for naming the person behind a credit. Read once: a
            // statement is thousands of lines and this would otherwise be a query per line.
            var funds = await LoadFundIndex(lines);

            int changed = 0;
            foreach (var line in lines)
            {
                // A person's decision is final. A rule added next month must not quietly overwrite
                // what somebody looked at the receipt and decided.
                if (line.CategorySetByUser) continue;

                string narration = $"{line.Description} {line.ReferenceNo}".Trim();
                string side = line.Debit > 0m ? BankLineDirection.Debit : BankLineDirection.Credit;
                decimal amount = line.Debit > 0m ? line.Debit : line.Credit;

                var hit = ordered.FirstOrDefault(r => Matches(r, narration, side, amount));

                string category = hit != null
                    ? BankLineCategory.Normalize(hit.Category)
                    : BankLineCategory.Uncategorised;

                // Nothing matched: a credit that answers a known deposit is still a user's funding,
                // and saying so without a rule is better than making every operator write the rule
                // for the one thing this system already knows.
                int counterparty = 0;
                string counterpartyName = string.Empty;

                if (TryFindFund(funds, line, out var fund))
                {
                    counterparty = fund.UserID;
                    counterpartyName = fund.UserName;
                    if (category == BankLineCategory.Uncategorised && line.Credit > 0m)
                        category = BankLineCategory.UserFund;
                }

                if (line.Category != category || line.CategoryRuleID != (hit?.BankLineCategoryRuleID ?? 0)
                    || line.CounterpartyUserID != counterparty)
                {
                    changed++;
                }

                line.Category = category;
                line.CategoryRuleID = hit?.BankLineCategoryRuleID ?? 0;
                line.CounterpartyUserID = counterparty;
                line.CounterpartyName = Trim(counterpartyName, 150);
                line.UpdatedateTime = DateTime.UtcNow;
            }

            return changed;
        }

        public async Task<int> FlagDuplicates(List<BankStatementLine> lines, int bankAccountId)
        {
            if (lines.Count == 0) return 0;

            int flagged = 0;

            // ── 1. The same reference and amount twice on the statement ────────────────────
            //
            // A bank does not normally print a line twice, so this is almost always a statement
            // uploaded over itself, or a genuine double credit. Either way the operator has to see
            // it before the second one is treated as new money.
            foreach (var group in lines
                .Where(l => l.ReferenceKey.Length > 0)
                .GroupBy(l => (l.ReferenceKey, Amount: l.Debit > 0m ? -l.Debit : l.Credit))
                .Where(g => g.Count() > 1))
            {
                var ordered = group.OrderBy(l => l.TxnDate).ThenBy(l => l.RowNo).ToList();
                var first = ordered[0];
                foreach (var dup in ordered.Skip(1))
                {
                    dup.DuplicateOfLineID = first.BankStatementLineID;
                    dup.DuplicateReason = Trim(
                        $"Same reference {first.ReferenceNo} and amount as the line dated {first.TxnDate:dd MMM yyyy} "
                        + "earlier in this statement.", 250);
                    dup.UpdatedateTime = DateTime.UtcNow;
                    flagged++;
                }
            }

            // ── 2. A deposit claimed by more than one fund request ─────────────────────────
            //
            // This is the one that costs money: a retailer raises a fund request against a UTR,
            // it is approved, and the same UTR is raised again. The bank credited once; the wallet
            // was funded twice. The statement is where it becomes visible.
            var funds = await LoadFundIndex(lines);
            foreach (var line in lines.Where(l => l.Credit > 0m && l.ReferenceKey.Length > 0))
            {
                if (!funds.TryGetValue(line.ReferenceKey, out var claims) || claims.Count <= 1) continue;

                var approved = claims.Where(c => c.IsApproved).ToList();
                if (approved.Count <= 1) continue;

                line.DuplicateReason = Trim(
                    $"{approved.Count} approved fund requests claim this deposit "
                    + $"({string.Join(", ", approved.Take(4).Select(c => $"#{c.FundTransactionID} {c.UserName} {c.Amount:0.00}"))})"
                    + ". The bank credited it once.", 250);
                if (line.DuplicateOfLineID == 0) line.DuplicateOfLineID = line.BankStatementLineID; // self: the flag is about the claims, not another line
                line.UpdatedateTime = DateTime.UtcNow;
                flagged++;
            }

            return flagged;
        }

        // ── Matching ────────────────────────────────────────────────────────────────────────

        private static bool Matches(BankLineCategoryRule rule, string narration, string side, decimal amount)
        {
            string dir = BankLineDirection.Normalize(rule.Direction);
            if (dir != BankLineDirection.Any && dir != side) return false;

            if (rule.MinAmount > 0m && amount < rule.MinAmount) return false;
            if (rule.MaxAmount > 0m && amount > rule.MaxAmount) return false;

            string pattern = (rule.Pattern ?? string.Empty).Trim();
            if (pattern.Length == 0) return false;

            try
            {
                return BankLineMatchType.Normalize(rule.MatchType) switch
                {
                    BankLineMatchType.StartsWith => narration.StartsWith(pattern, StringComparison.OrdinalIgnoreCase),
                    BankLineMatchType.Regex => Regex.IsMatch(narration, pattern,
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(250)),
                    _ => narration.Contains(pattern, StringComparison.OrdinalIgnoreCase),
                };
            }
            catch (RegexMatchTimeoutException)
            {
                // A pattern that runs away must not stall an import of ten thousand lines.
                return false;
            }
            catch (ArgumentException)
            {
                // An invalid regular expression somebody typed. It matches nothing rather than
                // throwing out the whole statement.
                return false;
            }
        }

        /// <summary>A bank-specific rule outranks a general one; everything else is the tie-break.</summary>
        private static int DerivedPriority(BankLineCategoryRule rule)
        {
            int p = rule.BankAccountID > 0 ? 200 : 100;
            if (BankLineDirection.Normalize(rule.Direction) != BankLineDirection.Any) p += 20;
            if (rule.MinAmount > 0m || rule.MaxAmount > 0m) p += 10;
            return p;
        }

        // ── Who the line is about ───────────────────────────────────────────────────────────

        private sealed record FundClaim(int FundTransactionID, int UserID, string UserName, decimal Amount, bool IsApproved);

        /// <summary>
        /// Deposits indexed by their normalised UTR, around the dates this statement covers.
        ///
        /// The window is six weeks either side, not a few days. A UTR match is exact, so the dates
        /// only bound the query — and a retailer who pays on the 2nd and raises the request on the
        /// 20th is ordinary, not an edge case. A tight window silently leaves their deposit
        /// unnamed, which is the one thing this is here to do.
        /// </summary>
        private async Task<Dictionary<string, List<FundClaim>>> LoadFundIndex(List<BankStatementLine> lines)
        {
            var index = new Dictionary<string, List<FundClaim>>(StringComparer.OrdinalIgnoreCase);
            try
            {
                DateTime from = lines.Min(l => l.TxnDate).AddDays(-45);
                DateTime to = lines.Max(l => l.TxnDate).AddDays(46);

                var rows = await _context.FundTransactions.AsNoTracking()
                    .Where(f => !f.IsDeleted && f.DateTime >= from && f.DateTime < to
                             && f.UTRNumber != null && f.UTRNumber != string.Empty)
                    .Select(f => new
                    {
                        f.FundTransactionID,
                        f.SenderID,
                        f.Amount,
                        f.UTRNumber,
                        f.Status,
                    })
                    .ToListAsync();

                if (rows.Count == 0) return index;

                var userIds = rows.Select(r => r.SenderID).Distinct().ToList();
                var names = await _context.UserMasters.AsNoTracking()
                    .Where(u => userIds.Contains(u.UserMasterID))
                    .Select(u => new { u.UserMasterID, u.PersonName, u.FirmName, u.AliasName })
                    .ToListAsync();
                var nameById = names.ToDictionary(
                    u => u.UserMasterID,
                    u =>
                    {
                        string n = !string.IsNullOrWhiteSpace(u.FirmName) ? u.FirmName!
                                 : !string.IsNullOrWhiteSpace(u.PersonName) ? u.PersonName!
                                 : $"user {u.UserMasterID}";
                        return string.IsNullOrWhiteSpace(u.AliasName) ? n : $"{n} ({u.AliasName})";
                    });

                foreach (var r in rows)
                {
                    string key = NormaliseRef(r.UTRNumber ?? string.Empty);
                    if (key.Length == 0) continue;

                    if (!index.TryGetValue(key, out var list))
                    {
                        list = new List<FundClaim>();
                        index[key] = list;
                    }

                    list.Add(new FundClaim(
                        r.FundTransactionID,
                        r.SenderID,
                        nameById.TryGetValue(r.SenderID, out var nm) ? nm : $"user {r.SenderID}",
                        r.Amount,
                        IsApprovedStatus(r.Status)));
                }
            }
            catch (Exception ex)
            {
                // Naming a counterparty is a nicety; failing to do it must not fail an import.
                _logger.Write(ex);
            }

            return index;
        }

        private static bool TryFindFund(
            Dictionary<string, List<FundClaim>> index, BankStatementLine line, out FundClaim claim)
        {
            claim = default!;
            if (line.Credit <= 0m) return false;

            string key = line.ReferenceKey;
            if (key.Length == 0)
            {
                var m = RefInText.Match(line.Description ?? string.Empty);
                if (!m.Success) return false;
                key = NormaliseRef(m.Value);
            }

            if (!index.TryGetValue(key, out var claims) || claims.Count == 0) return false;

            // Prefer a claim for the same amount: a UTR can be typed against the wrong request.
            claim = claims.FirstOrDefault(c => Math.Abs(c.Amount - line.Credit) <= 0.5m) ?? claims[0];
            return true;
        }

        private static bool IsApprovedStatus(string? status)
        {
            string s = (status ?? string.Empty).Trim().ToUpperInvariant();
            return s is "SUCCESS" or "APPROVED" or "ACCEPT" or "ACCEPTED" or "COMPLETE" or "COMPLETED";
        }

        /// <summary>
        /// A reference as a key: letters and digits only, upper-cased, so "UTR 1234-5678" and
        /// "utr12345678" are one thing. Matches what the importer stores in ReferenceKey.
        /// </summary>
        private static string NormaliseRef(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference)) return string.Empty;
            var chars = reference.Where(char.IsLetterOrDigit).ToArray();
            return new string(chars).ToUpperInvariant();
        }

        private static string Trim(string? s, int max)
            => string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= max ? s : s.Substring(0, max));
    }
}
