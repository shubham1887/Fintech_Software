using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Phase5;
using FintechApp.Responses.Phase5;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="IBankReconService" />
    public class BankReconService : IBankReconService
    {
        private readonly ApplicationDbContext _context;
        private readonly Logger _logger = new();

        public BankReconService(ApplicationDbContext context) => _context = context;

        /// <summary>One side of the comparison, reduced to what matching actually needs. Both the
        /// statement and the ledger are projected into this so the two passes below read the same
        /// for either side.</summary>
        private sealed class Movement
        {
            public int Id;                 // BankStatementLineID or AcctVoucherLineID
            public DateTime Date;
            /// <summary>The statement's own reference, normalised. Empty on the ledger side.</summary>
            public string RefKey = string.Empty;
            /// <summary>The identifier-shaped tokens found in a voucher narration. Empty on the
            /// bank side, which has a reference column of its own.</summary>
            public List<string> RefTokens = new();
            /// <summary>Signed, and + is ALWAYS money arriving. This is the single convention that
            /// keeps a receipt from pairing with a payment of the same size.</summary>
            public decimal Signed;
            public string Text = string.Empty;
            public int VoucherId;
            public string VoucherNo = string.Empty;
            public decimal Debit, Credit;
            /// <summary>What the line IS, who it is about, and whether it looks like the same money
            /// twice. Carried through so the register reads as "a purchase from JaiJui" rather than
            /// as an unexplained debit - matching says it happened, this says what it was.</summary>
            public string Category = string.Empty;
            public string Counterparty = string.Empty;
            public string DuplicateReason = string.Empty;
        }

        public async Task<(BankReconResult? Result, string Failure)> Reconcile(BankReconRequest request, bool commit)
        {
            if (request.BankAccountID <= 0) return (null, "A bank account is required.");
            if (request.PeriodFrom == default || request.PeriodTo == default)
                return (null, "A period is required.");
            if (request.PeriodTo.Date < request.PeriodFrom.Date)
                return (null, "The period ends before it starts.");

            var account = await _context.BankAccounts.AsNoTracking()
                .FirstOrDefaultAsync(a => a.BankAccountID == request.BankAccountID && !a.IsDeleted);
            if (account == null) return (null, "That bank account does not exist.");

            decimal tol = request.Tolerance > 0 ? request.Tolerance : BankReconTolerance.AmountRupees;
            int window = request.DateWindowDays > 0 ? request.DateWindowDays : BankReconTolerance.DateWindowDays;
            DateTime periodFrom = request.PeriodFrom.Date, periodTo = request.PeriodTo.Date;

            var result = new BankReconResult
            {
                BankAccountID = account.BankAccountID,
                BankAccountName = account.AccountName,
                LedgerAccountCode = account.LedgerAccountCode,
                PeriodFrom = periodFrom,
                PeriodTo = periodTo,
                Tolerance = tol,
                DateWindowDays = window,
                Committed = commit,
            };

            // ── The bank's side ───────────────────────────────────────────────────────────
            var bankLines = await _context.BankStatementLines
                .Where(l => !l.IsDeleted && l.BankAccountID == account.BankAccountID
                         && l.TxnDate >= periodFrom && l.TxnDate <= periodTo)
                .Join(_context.BankStatementImports.Where(i => !i.IsDeleted && i.Status == BankStatementStatus.Active),
                      l => l.BankStatementImportID, i => i.BankStatementImportID, (l, i) => l)
                .ToListAsync();

            result.NoStatementImported = bankLines.Count == 0;

            // ── The ledger's side: every posted voucher line on this account's ledger code ──
            var ledgerRows = await (
                from vl in _context.AcctVoucherLines.AsNoTracking()
                join v in _context.AcctVouchers.AsNoTracking() on vl.AcctVoucherID equals v.AcctVoucherID
                where !vl.IsDeleted && !v.IsDeleted
                   && vl.AccountCode == account.LedgerAccountCode
                   && v.VoucherDate >= periodFrom && v.VoucherDate <= periodTo
                select new
                {
                    vl.AcctVoucherLineID, vl.AcctVoucherID, v.VoucherNo, v.VoucherDate,
                    vl.Debit, vl.Credit,
                    Narration = string.IsNullOrEmpty(vl.Narration) ? v.Narration : vl.Narration,
                }).ToListAsync();

            // Pairings already made on an earlier run. A ledger line matched last month must not be
            // offered again this month — that is the whole reason the match is its own table.
            var existing = await _context.BankReconMatches.AsNoTracking()
                .Where(m => !m.IsDeleted && m.BankAccountID == account.BankAccountID)
                .ToListAsync();
            var matchedBankIds = existing.Select(m => m.BankStatementLineID).ToHashSet();
            var matchedLedgerIds = existing.Select(m => m.AcctVoucherLineID).ToHashSet();
            var existingByBankId = existing.ToDictionary(m => m.BankStatementLineID, m => m);

            var bank = bankLines.Select(l => new Movement
            {
                Id = l.BankStatementLineID,
                Date = l.TxnDate,
                RefKey = l.ReferenceKey,
                // Credit on a statement is money IN.
                Signed = Math.Round(l.Credit - l.Debit, 2),
                Text = l.Description,
                Category = l.Category ?? string.Empty,
                Counterparty = l.CounterpartyName ?? string.Empty,
                DuplicateReason = l.DuplicateReason ?? string.Empty,
                Debit = l.Debit,
                Credit = l.Credit,
            }).ToList();

            var ledger = ledgerRows.Select(r => new Movement
            {
                Id = r.AcctVoucherLineID,
                Date = r.VoucherDate,
                // A DEBIT to Bank is money IN — the mirror of the statement's credit.
                Signed = Math.Round(r.Debit - r.Credit, 2),
                // Not the whole narration normalised — the identifier-shaped tokens INSIDE it. A
                // narration reads "UPI vendor payment UTR 778899001122" while the bank's reference
                // column holds the bare number, so comparing the two as whole strings never matches.
                RefTokens = BankStatementImportService.ReferenceTokens(r.Narration).ToList(),
                Text = r.Narration ?? string.Empty,
                VoucherId = r.AcctVoucherID,
                VoucherNo = r.VoucherNo,
                Debit = r.Debit,
                Credit = r.Credit,
            }).ToList();

            var bankById = bank.ToDictionary(m => m.Id);
            var ledgerById = ledger.ToDictionary(m => m.Id);

            var pairs = new List<(Movement Bank, Movement Ledger, string Method)>();
            var usedBank = new HashSet<int>();
            var usedLedger = new HashSet<int>();

            // Rows paired on a previous committed run stay paired, so a re-run is stable.
            foreach (var m in existing)
            {
                if (!bankById.TryGetValue(m.BankStatementLineID, out var b)) continue;
                if (!ledgerById.TryGetValue(m.AcctVoucherLineID, out var l)) continue;
                pairs.Add((b, l, m.Method));
                usedBank.Add(b.Id);
                usedLedger.Add(l.Id);
            }

            // ── Pass 1: the bank's reference, found on the voucher ────────────────────────
            // The only pairing safe without a human looking at it. The ledger side's "reference" is
            // whatever digits its narration carries, so a narration that quotes the UTR matches.
            var ledgerByRef = new Dictionary<string, List<Movement>>(StringComparer.Ordinal);
            foreach (var l in ledger)
            {
                if (usedLedger.Contains(l.Id)) continue;
                foreach (string token in l.RefTokens)
                {
                    if (!ledgerByRef.TryGetValue(token, out var list)) ledgerByRef[token] = list = new();
                    list.Add(l);
                }
            }

            foreach (var b in bank)
            {
                if (usedBank.Contains(b.Id) || b.RefKey.Length < 6) continue;
                if (!ledgerByRef.TryGetValue(b.RefKey, out var candidates)) continue;

                var pick = candidates.FirstOrDefault(c => !usedLedger.Contains(c.Id));
                if (pick == null) continue;

                pairs.Add((b, pick, BankMatchMethod.Reference));
                usedBank.Add(b.Id);
                usedLedger.Add(pick.Id);
            }

            // ── Pass 2: same signed amount, dates within the window, and UNAMBIGUOUS ──────
            // Only when exactly one unmatched movement of that amount is left on each side. Two
            // ₹5,000 payments on the same day are indistinguishable without a reference, and
            // pairing them arbitrarily would produce a reconciliation that balances while being
            // wrong about which payment is which.
            var ledgerByAmount = ledger.Where(l => !usedLedger.Contains(l.Id))
                                       .GroupBy(l => l.Signed)
                                       .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var b in bank.Where(x => !usedBank.Contains(x.Id)))
            {
                if (!ledgerByAmount.TryGetValue(b.Signed, out var candidates)) continue;

                var inWindow = candidates
                    .Where(c => !usedLedger.Contains(c.Id) && Math.Abs((c.Date - b.Date).TotalDays) <= window)
                    .ToList();
                if (inWindow.Count != 1) continue;

                // And the other way round: this ledger line must not also fit another bank line.
                var ledgerPick = inWindow[0];
                int competing = bank.Count(x => !usedBank.Contains(x.Id) && x.Id != b.Id
                                             && x.Signed == b.Signed
                                             && Math.Abs((ledgerPick.Date - x.Date).TotalDays) <= window);
                if (competing > 0) continue;

                pairs.Add((b, ledgerPick, BankMatchMethod.AmountAndDate));
                usedBank.Add(b.Id);
                usedLedger.Add(ledgerPick.Id);
            }

            // ── Build the buckets ─────────────────────────────────────────────────────────
            var rows = new List<BankReconRow>();

            foreach (var (b, l, method) in pairs)
            {
                decimal variance = Math.Round(b.Signed - l.Signed, 2);
                bool within = Math.Abs(variance) <= tol;
                rows.Add(new BankReconRow
                {
                    Bucket = within ? BankReconBucket.Matched : BankReconBucket.Mismatch,
                    BankStatementLineID = b.Id,
                    BankDate = b.Date,
                    Description = b.Text,
                    ReferenceNo = b.RefKey,
                    Category = b.Category,
                    Counterparty = b.Counterparty,
                    DuplicateReason = b.DuplicateReason,
                    BankDebit = b.Debit,
                    BankCredit = b.Credit,
                    AcctVoucherLineID = l.Id,
                    AcctVoucherID = l.VoucherId,
                    VoucherNo = l.VoucherNo,
                    VoucherDate = l.Date,
                    Narration = l.Text,
                    LedgerDebit = l.Debit,
                    LedgerCredit = l.Credit,
                    Variance = variance,
                    Method = method,
                    DateGapDays = (int)Math.Abs((l.Date - b.Date).TotalDays),
                });
            }

            foreach (var b in bank.Where(x => !usedBank.Contains(x.Id)))
            {
                rows.Add(new BankReconRow
                {
                    Bucket = BankReconBucket.MissingInBooks,
                    BankStatementLineID = b.Id,
                    BankDate = b.Date,
                    Description = b.Text,
                    ReferenceNo = b.RefKey,
                    Category = b.Category,
                    Counterparty = b.Counterparty,
                    DuplicateReason = b.DuplicateReason,
                    BankDebit = b.Debit,
                    BankCredit = b.Credit,
                    Variance = b.Signed,
                });
            }

            foreach (var l in ledger.Where(x => !usedLedger.Contains(x.Id)))
            {
                rows.Add(new BankReconRow
                {
                    Bucket = BankReconBucket.MissingInBank,
                    AcctVoucherLineID = l.Id,
                    AcctVoucherID = l.VoucherId,
                    VoucherNo = l.VoucherNo,
                    VoucherDate = l.Date,
                    Narration = l.Text,
                    LedgerDebit = l.Debit,
                    LedgerCredit = l.Credit,
                    Variance = -l.Signed,
                });
            }

            // The ledger only knows a movement once a voucher is posted, and on an install where
            // the Bank account carries few vouchers every line lands in "in bank, not in books".
            // The operator's OWN records still explain most of them — a retailer's deposit is an
            // approved fund request, a debit to a provider is a Provider Purchase entry — so each
            // unmatched bank line is read against those before it is called unresolved.
            try { await ExplainFromOperations(rows, window); }
            catch (Exception ex) { _logger.Write(ex); }

            result.Rows = rows
                .OrderBy(r => BucketOrder(r.Bucket))
                .ThenBy(r => r.BankDate ?? r.VoucherDate ?? DateTime.MinValue)
                .ToList();

            result.Summary = rows
                .GroupBy(r => r.Bucket)
                .Select(g => new BankReconBucketSummary
                {
                    Bucket = g.Key,
                    Count = g.Count(),
                    BankAmount = Math.Round(g.Sum(x => x.BankCredit - x.BankDebit), 2),
                    LedgerAmount = Math.Round(g.Sum(x => x.LedgerDebit - x.LedgerCredit), 2),
                })
                .OrderBy(s => BucketOrder(s.Bucket)).ToList();

            // ── The reconciliation statement ──────────────────────────────────────────────
            // The bank's closing balance from the statement itself when it carried one; otherwise
            // the movement total, which is all we can honestly claim.
            var lastWithBalance = bankLines
                .Where(l => l.RunningBalance.HasValue)
                .OrderBy(l => l.TxnDate).ThenBy(l => l.RowNo).LastOrDefault();
            result.BankClosingBalance = lastWithBalance?.RunningBalance
                                        ?? Math.Round(bankLines.Sum(l => l.Credit - l.Debit), 2);

            // The ledger's Bank balance to the same date — from the beginning, not just this
            // period, or it is not a balance.
            result.LedgerClosingBalance = await LedgerBalanceTo(account.LedgerAccountCode, periodTo);

            result.Difference = Math.Round(result.BankClosingBalance - result.LedgerClosingBalance, 2);
            // The difference is "explained" when the two one-sided buckets account for it exactly —
            // which is what a reconciliation statement asserts.
            decimal unexplained = Math.Round(
                result.Difference
                - rows.Where(r => r.Bucket == BankReconBucket.MissingInBooks).Sum(r => r.BankCredit - r.BankDebit)
                + rows.Where(r => r.Bucket == BankReconBucket.MissingInBank).Sum(r => r.LedgerDebit - r.LedgerCredit), 2);
            result.Explained = Math.Abs(unexplained) <= tol;

            if (commit)
            {
                try
                {
                    await Persist(account, pairs, bank, tol, request.CreatedByUserID, existingByBankId);
                }
                catch (Exception ex)
                {
                    _logger.Write(ex);
                    return (null, "The match could not be saved. Nothing was changed.");
                }
            }

            return (result, string.Empty);
        }

        /// <summary>
        /// Resolution for the "in bank, not in books" rows, from the operational tables:
        ///  • a CREDIT pairs with an approved fund request (same UTR, else the one request of that
        ///    amount within the date window) → Resolved, "Fund request #n from <firm>";
        ///  • a DEBIT pairs with a Provider Purchase entry of that amount within the window (and of
        ///    that provider, when the line names one) → Resolved, "Provider purchase #n — <provider>";
        ///  • a line a person categorised as an expense / bank charge / transfer / interest /
        ///    settlement / refund is Resolved by that decision;
        ///  • a line marked PURCHASE with no purchase entry stays Pending until the entry is made —
        ///    the entry is what moves the provider stock, so the category alone is not enough;
        ///  • anything else is Pending with what it needs.
        /// Each fund request / purchase explains one line only, so two deposits of ₹5,000 are
        /// never both cleared by one request.
        /// </summary>
        private async Task ExplainFromOperations(List<BankReconRow> rows, int window)
        {
            var open = rows.Where(r => r.Bucket == BankReconBucket.MissingInBooks && r.BankStatementLineID.HasValue && r.BankDate.HasValue).ToList();
            if (open.Count == 0) return;

            DateTime winFrom = open.Min(r => r.BankDate!.Value).AddDays(-window);
            DateTime winTo = open.Max(r => r.BankDate!.Value).AddDays(window + 1);

            var requests = await (
                from f in _context.FundTransactions.AsNoTracking()
                join u in _context.UserMasters.AsNoTracking() on f.ReceiverID equals u.UserMasterID into ru
                from u in ru.DefaultIfEmpty()
                where !f.IsDelete && f.SenderID != f.ReceiverID
                   && f.TransactionDOC >= winFrom && f.TransactionDOC < winTo
                   && (f.Status == "SUCCESS" || f.Status == "APPROVED" || f.Status == "COMPLETED")
                   && (f.Through == "REQUEST_TRANSFER" || f.Through == "PG-TRANSFER" || f.Through == "E-COLLECTION")
                select new { f.FundTransactionID, f.Amount, f.UTRNumber, f.TransactionDOC, Firm = u != null ? u.FirmName : string.Empty })
                .ToListAsync();
            var purchases = await (
                from p in _context.ProviderPurchases.AsNoTracking()
                join pm in _context.ProviderMasters.AsNoTracking() on p.ProviderID equals pm.ProviderID into pp
                from pm in pp.DefaultIfEmpty()
                where !p.IsDelete && p.PurchaseDate >= winFrom && p.PurchaseDate < winTo
                select new { p.ProviderPurchaseID, p.Amount, p.PurchaseDate, Provider = pm != null ? pm.ProviderName : string.Empty })
                .ToListAsync();

            var usedRequests = new HashSet<int>();
            var usedPurchases = new HashSet<int>();
            string Key(string? s) => new string((s ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

            foreach (var r in open.OrderBy(r => r.BankDate))
            {
                DateTime day = r.BankDate!.Value.Date;
                bool inWindow(DateTime d) => Math.Abs((d.Date - day).TotalDays) <= window;
                string cat = (r.Category ?? string.Empty).ToUpperInvariant();

                if (r.BankCredit > 0m)
                {
                    var candidates = requests.Where(q => !usedRequests.Contains(q.FundTransactionID)
                                                         && Math.Abs(q.Amount - r.BankCredit) <= BankReconTolerance.AmountRupees).ToList();
                    var byRef = r.ReferenceNo.Length >= 6 ? candidates.FirstOrDefault(q => Key(q.UTRNumber) == Key(r.ReferenceNo)) : null;
                    var pick = byRef ?? (candidates.Count(q => inWindow(q.TransactionDOC)) == 1 ? candidates.First(q => inWindow(q.TransactionDOC)) : null);
                    if (pick != null)
                    {
                        usedRequests.Add(pick.FundTransactionID);
                        r.Resolution = "Resolved";
                        r.ResolutionNote = $"Fund request #{pick.FundTransactionID} from {pick.Firm}".Trim();
                        r.Method = byRef != null ? "FundRequest" : "FundRequestAmount";
                        r.VoucherNo = $"FR-{pick.FundTransactionID}";
                        r.VoucherDate = pick.TransactionDOC;
                        r.Narration = r.ResolutionNote;
                        r.DateGapDays = (int)Math.Abs((pick.TransactionDOC.Date - day).TotalDays);
                        if (string.IsNullOrWhiteSpace(r.Counterparty)) r.Counterparty = pick.Firm;
                        continue;
                    }
                }
                else if (r.BankDebit > 0m && (cat == BankLineCategory.Purchase || cat == BankLineCategory.Uncategorised || cat.Length == 0))
                {
                    var candidates = purchases.Where(p => !usedPurchases.Contains(p.ProviderPurchaseID)
                                                          && Math.Abs(p.Amount - r.BankDebit) <= BankReconTolerance.AmountRupees
                                                          && inWindow(p.PurchaseDate)).ToList();
                    if (cat == BankLineCategory.Purchase && !string.IsNullOrWhiteSpace(r.Counterparty))
                        candidates = candidates.Where(p => string.Equals(p.Provider, r.Counterparty, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (candidates.Count == 1)
                    {
                        var pick = candidates[0];
                        usedPurchases.Add(pick.ProviderPurchaseID);
                        r.Resolution = "Resolved";
                        r.ResolutionNote = $"Provider purchase #{pick.ProviderPurchaseID} — {pick.Provider}".Trim();
                        r.Method = "ProviderPurchase";
                        r.VoucherNo = $"PP-{pick.ProviderPurchaseID}";
                        r.VoucherDate = pick.PurchaseDate;
                        r.Narration = r.ResolutionNote;
                        r.DateGapDays = (int)Math.Abs((pick.PurchaseDate.Date - day).TotalDays);
                        if (string.IsNullOrWhiteSpace(r.Counterparty)) r.Counterparty = pick.Provider;
                        if (cat != BankLineCategory.Purchase) r.Category = BankLineCategory.Purchase;
                        continue;
                    }
                }

                switch (cat)
                {
                    case BankLineCategory.Purchase:
                        r.Resolution = "Pending";
                        r.ResolutionNote = string.IsNullOrWhiteSpace(r.Counterparty)
                            ? "Provider purchase entry not recorded yet — pick the provider and add it on Provider Purchases"
                            : $"Provider purchase entry for {r.Counterparty} not recorded yet";
                        break;
                    case BankLineCategory.UserFund:
                        r.Resolution = "Pending";
                        r.ResolutionNote = string.IsNullOrWhiteSpace(r.Counterparty)
                            ? "No approved fund request of this amount — approve the request or pick the user"
                            : $"No approved fund request from {r.Counterparty} for this amount";
                        break;
                    case BankLineCategory.Expense:
                    case BankLineCategory.BankCharges:
                    case BankLineCategory.Transfer:
                    case BankLineCategory.Interest:
                    case BankLineCategory.Settlement:
                    case BankLineCategory.Refund:
                        r.Resolution = "Resolved";
                        r.ResolutionNote = $"Categorised as {cat.Replace('_', ' ').ToLowerInvariant()}";
                        break;
                    default:
                        r.Resolution = "Pending";
                        r.ResolutionNote = r.BankDebit > 0m
                            ? "Needs a category — provider purchase, expense, bank charge or transfer"
                            : "Needs a category — user fund, settlement, interest or refund";
                        break;
                }
            }
        }

        /// <summary>Write the pairings, and stamp the statement lines so the register can filter.</summary>
        private async Task Persist(
            BankAccount account,
            List<(Movement Bank, Movement Ledger, string Method)> pairs,
            List<Movement> bank,
            decimal tol,
            int userId,
            Dictionary<int, BankReconMatch> existingByBankId)
        {
            foreach (var (b, l, method) in pairs)
            {
                decimal variance = Math.Round(b.Signed - l.Signed, 2);
                string status = Math.Abs(variance) <= tol ? BankReconBucket.Matched : BankReconBucket.Mismatch;

                if (existingByBankId.TryGetValue(b.Id, out var already))
                {
                    var tracked = await _context.BankReconMatches
                        .FirstOrDefaultAsync(m => m.BankReconMatchID == already.BankReconMatchID);
                    if (tracked != null)
                    {
                        tracked.AcctVoucherLineID = l.Id;
                        tracked.Variance = variance;
                        tracked.UpdatedateTime = DateTime.UtcNow;
                    }
                }
                else
                {
                    _context.BankReconMatches.Add(new BankReconMatch
                    {
                        BankStatementLineID = b.Id,
                        AcctVoucherLineID = l.Id,
                        BankAccountID = account.BankAccountID,
                        Method = method,
                        Variance = variance,
                        MatchedByUserID = userId,
                        DateTime = DateTime.UtcNow,
                        UpdatedateTime = DateTime.UtcNow,
                        IsDeleted = false,
                    });
                }

                var line = await _context.BankStatementLines
                    .FirstOrDefaultAsync(x => x.BankStatementLineID == b.Id);
                if (line != null)
                {
                    line.MatchStatus = status;
                    line.Variance = variance;
                    line.UpdatedateTime = DateTime.UtcNow;
                }
            }

            // Anything this run did NOT pair goes back to Unmatched, so a line that was matched by
            // an earlier run against a voucher since reversed does not stay green on the register.
            var pairedIds = pairs.Select(p => p.Bank.Id).ToHashSet();
            foreach (var b in bank.Where(x => !pairedIds.Contains(x.Id)))
            {
                var line = await _context.BankStatementLines
                    .FirstOrDefaultAsync(x => x.BankStatementLineID == b.Id);
                if (line == null || line.MatchStatus == "Unmatched") continue;
                line.MatchStatus = "Unmatched";
                line.Variance = 0m;
                line.UpdatedateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>The ledger balance on an account up to and including a date: debits − credits,
        /// over every posted voucher since the books began.</summary>
        private async Task<decimal> LedgerBalanceTo(string accountCode, DateTime upTo)
        {
            var sums = await (
                from vl in _context.AcctVoucherLines.AsNoTracking()
                join v in _context.AcctVouchers.AsNoTracking() on vl.AcctVoucherID equals v.AcctVoucherID
                where !vl.IsDeleted && !v.IsDeleted && vl.AccountCode == accountCode && v.VoucherDate <= upTo
                select new { vl.Debit, vl.Credit }).ToListAsync();
            return Math.Round(sums.Sum(x => x.Debit - x.Credit), 2);
        }

        public async Task<string> ManualMatch(BankManualMatchRequest request)
        {
            if (request.BankStatementLineID <= 0 || request.AcctVoucherLineID <= 0)
                return "Both a statement line and a ledger line are required.";

            var line = await _context.BankStatementLines
                .FirstOrDefaultAsync(l => l.BankStatementLineID == request.BankStatementLineID && !l.IsDeleted);
            if (line == null) return "That statement line does not exist.";

            var ledger = await (
                from vl in _context.AcctVoucherLines.AsNoTracking()
                join v in _context.AcctVouchers.AsNoTracking() on vl.AcctVoucherID equals v.AcctVoucherID
                where vl.AcctVoucherLineID == request.AcctVoucherLineID && !vl.IsDeleted && !v.IsDeleted
                select new { vl.Debit, vl.Credit }).FirstOrDefaultAsync();
            if (ledger == null) return "That ledger line does not exist.";

            // The uniqueness is enforced in the database too; checking here turns a 500 into a
            // sentence the person can act on.
            bool bankTaken = await _context.BankReconMatches
                .AnyAsync(m => m.BankStatementLineID == line.BankStatementLineID && !m.IsDeleted);
            if (bankTaken) return "That statement line is already matched. Unmatch it first.";

            bool ledgerTaken = await _context.BankReconMatches
                .AnyAsync(m => m.AcctVoucherLineID == request.AcctVoucherLineID && !m.IsDeleted);
            if (ledgerTaken) return "That ledger line is already matched to another statement line.";

            decimal variance = Math.Round((line.Credit - line.Debit) - (ledger.Debit - ledger.Credit), 2);

            _context.BankReconMatches.Add(new BankReconMatch
            {
                BankStatementLineID = line.BankStatementLineID,
                AcctVoucherLineID = request.AcctVoucherLineID,
                BankAccountID = line.BankAccountID,
                Method = BankMatchMethod.Manual,
                Variance = variance,
                MatchedByUserID = request.CreatedByUserID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false,
            });

            line.MatchStatus = Math.Abs(variance) <= BankReconTolerance.AmountRupees
                ? BankReconBucket.Matched : BankReconBucket.Mismatch;
            line.Variance = variance;
            line.UpdatedateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        public async Task<string> Unmatch(BankUnmatchRequest request)
        {
            if (request.BankStatementLineID <= 0) return "A statement line is required.";

            var matches = await _context.BankReconMatches
                .Where(m => m.BankStatementLineID == request.BankStatementLineID && !m.IsDeleted)
                .ToListAsync();
            if (matches.Count == 0) return "That statement line is not matched.";

            foreach (var m in matches)
            {
                m.IsDeleted = true;
                m.UpdatedateTime = DateTime.UtcNow;
            }

            var line = await _context.BankStatementLines
                .FirstOrDefaultAsync(l => l.BankStatementLineID == request.BankStatementLineID);
            if (line != null)
            {
                line.MatchStatus = "Unmatched";
                line.Variance = 0m;
                line.UpdatedateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return StatusType.Ok;
        }

        /// <summary>Matched first, then the two gaps, then the disagreements — the order an
        /// accountant works them in.</summary>
        private static int BucketOrder(string bucket) => bucket switch
        {
            BankReconBucket.Matched => 0,
            BankReconBucket.MissingInBooks => 1,
            BankReconBucket.MissingInBank => 2,
            BankReconBucket.Mismatch => 3,
            _ => 9,
        };
    }

    /// <summary>The bank accounts we reconcile.</summary>
    public class BankAccountService : IBankAccountService
    {
        private readonly ApplicationDbContext _context;
        public BankAccountService(ApplicationDbContext context) => _context = context;

        public Task<List<BankAccount>> GetAll() =>
            _context.BankAccounts.AsNoTracking().Where(a => !a.IsDeleted)
                .OrderBy(a => a.BankAccountID).ToListAsync();

        public async Task<(int BankAccountID, string Failure)> Save(SaveBankAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.AccountName)) return (0, "The account needs a name.");
            string code = (request.LedgerAccountCode ?? string.Empty).Trim();
            if (code.Length == 0) return (0, "The ledger account code is required.");

            // The code must exist in the chart, or every reconciliation on this account would
            // compare the statement against an empty ledger and report the whole balance missing.
            bool codeExists = await _context.AcctAccounts
                .AnyAsync(a => a.Code == code && !a.IsDeleted);
            if (!codeExists) return (0, $"There is no account '{code}' in the chart of accounts.");

            BankAccount entity;
            if (request.BankAccountID > 0)
            {
                var found = await _context.BankAccounts
                    .FirstOrDefaultAsync(a => a.BankAccountID == request.BankAccountID && !a.IsDeleted);
                if (found == null) return (0, "That bank account does not exist.");
                entity = found;
            }
            else
            {
                entity = new BankAccount { DateTime = DateTime.UtcNow, IsDeleted = false };
                _context.BankAccounts.Add(entity);
            }

            entity.AccountName = request.AccountName.Trim();
            entity.BankName = (request.BankName ?? string.Empty).Trim();
            entity.AccountNumber = (request.AccountNumber ?? string.Empty).Trim();
            entity.IFSC = (request.IFSC ?? string.Empty).Trim().ToUpperInvariant();
            entity.LedgerAccountCode = code;
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (entity.BankAccountID, string.Empty);
        }
    }
}
