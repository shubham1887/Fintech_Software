using FintechApp.Data;
using FintechApp.Interfaces.Services;
using FintechApp.Responses.Phase4;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <inheritdoc cref="ILedgerIntegrityService" />
    public class LedgerIntegrityService : ILedgerIntegrityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IServiceKindResolver _serviceKinds;
        private const decimal Tolerance = 0.01m;

        public LedgerIntegrityService(ApplicationDbContext context, IServiceKindResolver serviceKinds)
        {
            _context = context;
            _serviceKinds = serviceKinds;
        }

        public async Task<TrialBalanceResult> TrialBalance(string periodKey)
        {
            // Only POSTED vouchers count; a reversed one is cancelled by its reversing voucher,
            // which is itself posted, so the pair nets to zero without excluding either.
            var vq = _context.AcctVouchers.AsNoTracking().Where(v => !v.IsDeleted && v.Status == VoucherStatus.Posted);
            if (!string.IsNullOrWhiteSpace(periodKey)) vq = vq.Where(v => v.PeriodKey == periodKey.Trim());
            var voucherIds = await vq.Select(v => v.AcctVoucherID).ToListAsync();

            var accounts = await _context.AcctAccounts.AsNoTracking().Where(a => !a.IsDeleted).ToListAsync();
            var lines = await _context.AcctVoucherLines.AsNoTracking()
                .Where(l => voucherIds.Contains(l.AcctVoucherID) && !l.IsDeleted).ToListAsync();

            var byAccount = lines.GroupBy(l => l.AcctAccountID)
                .ToDictionary(g => g.Key, g => (Dr: g.Sum(x => x.Debit), Cr: g.Sum(x => x.Credit)));

            var rows = new List<TrialBalanceRow>();
            decimal totalDr = 0m, totalCr = 0m;
            foreach (var a in accounts.OrderBy(a => a.Code))
            {
                if (!byAccount.TryGetValue(a.AcctAccountID, out var t)) continue;
                if (t.Dr == 0m && t.Cr == 0m) continue;
                totalDr += t.Dr; totalCr += t.Cr;
                decimal net = a.NormalBalance == AccountSide.Debit ? t.Dr - t.Cr : t.Cr - t.Dr;
                rows.Add(new TrialBalanceRow
                {
                    AccountCode = a.Code, AccountName = a.Name, AccountType = a.AccountType,
                    Debit = t.Dr, Credit = t.Cr, Balance = Math.Round(net, 2),
                });
            }

            return new TrialBalanceResult
            {
                PeriodKey = periodKey ?? string.Empty,
                TotalDebit = Math.Round(totalDr, 2),
                TotalCredit = Math.Round(totalCr, 2),
                Balanced = Math.Abs(totalDr - totalCr) <= Tolerance,
                Rows = rows,
            };
        }

        public async Task<IntegrityCheckResult> WalletVsControl()
        {
            // The ledger's figure: the credit balance of the User Wallet control account (a
            // liability — what we owe users).
            decimal ledger = await ControlBalance(AccountCode.UserWalletControl, AccountSide.Credit);
            // The operational figure: the sum of every user's live wallet balance. This reads
            // UserWalletMasters, which is the table the wallet actually moves (FundService and the
            // transaction path both debit and credit it). UserBalanceMaster is a legacy table that
            // carries no rows on a live install, so summing that one made this check report a
            // difference of zero against a ledger of zero — a pass that proved nothing, which is
            // the one thing an integrity check must never do.
            // HoldBalance is a frozen SLICE of Balance, not money on top of it, so it is not added.
            decimal operational = await _context.UserWalletMasters.AsNoTracking()
                .SumAsync(w => (decimal?)w.Balance) ?? 0m;

            decimal diff = Math.Round(operational - ledger, 2);
            return new IntegrityCheckResult
            {
                Check = "Wallet vs control account",
                LedgerFigure = Math.Round(ledger, 2),
                OperationalFigure = Math.Round(operational, 2),
                Difference = diff,
                Ok = Math.Abs(diff) <= Tolerance,
                Detail = $"User Wallet control ({AccountCode.UserWalletControl}) against the sum of live user wallet balances.",
            };
        }

        public async Task<IntegrityCheckResult> ProviderStockVsControl()
        {
            decimal ledger = await ControlBalance(AccountCode.ProviderStock, AccountSide.Debit);
            decimal operational = await _context.ProviderMasters.AsNoTracking()
                .Where(p => p.IsActive)
                .SumAsync(p => (decimal?)(p.RechargeBalance + p.DMTBalance)) ?? 0m;

            decimal diff = Math.Round(ledger - operational, 2);
            return new IntegrityCheckResult
            {
                Check = "Provider stock vs control account",
                LedgerFigure = Math.Round(ledger, 2),
                OperationalFigure = Math.Round(operational, 2),
                Difference = diff,
                Ok = Math.Abs(diff) <= Tolerance,
                Detail = $"Provider Stock control ({AccountCode.ProviderStock}) against the sum of provider recharge + DMT balances.",
            };
        }

        /// <summary>
        /// Every chain commission is a PAIR: credited to the downline, and debited from whoever funded it
        /// that funds it. Both legs carry the same amount in opposite directions, and so do the two
        /// postings that take them back, so for any one transaction on a chain-funded service the
        /// four legs must sum to zero — whether it settled, failed, or had a leg taken back.
        ///
        /// A non-zero sum is a leg that is not there. Either a commission nobody funded (the credit
        /// landed, the funding debit did not), or a funding reversal with nothing behind it (the
        /// refund credited the admin for a debit it never took). Both create money out of nothing,
        /// and on this install the second kind cost Rs 7.65 on txn #268 — unnoticed for four days,
        /// because the posting result was discarded and nothing ever looked afterwards.
        ///
        /// This reads the operational ledger and nothing else. The two control checks above compare
        /// against the voucher ledger and are red by construction until the opening balances are
        /// journalled, which is precisely why a Rs 7.65 break could hide inside them: it is noise
        /// next to a crore. This check is zero or it is wrong.
        /// </summary>
        public async Task<IntegrityCheckResult> ChainCommissionsFunded()
        {
            // One pass over the commission legs. The principal and the surcharges are excluded by
            // the "commission" filter, which leaves the four leg kinds per tier plus the provider
            // income rows; the prefix match below keeps only the legs that pair.
            var rows = await _context.TransactionDetailss.AsNoTracking()
                .Where(d => !d.IsDeleted
                            && d.TransactionDescription.StartsWith("Recharge")
                            && d.TransactionDescription.Contains("commission"))
                .Select(d => new
                {
                    d.ReferenceID,
                    d.ServiceID,
                    d.TransactionDescription,
                    Delta = d.ClosingBalance - d.OpeningBalance,
                })
                .ToListAsync();

            // Only chain-funded services have a funding leg at all. A bill payment credits its
            // downline and posts a single net admin commission instead, so measuring it here would
            // report a break on every one of them — a check that is red by design proves nothing.
            var chainFunded = new Dictionary<int, bool>();
            foreach (int serviceID in rows.Select(r => r.ServiceID).Distinct())
                chainFunded[serviceID] = await _serviceKinds.IsChainFundedService(serviceID);

            // Net every tier's four legs per transaction. The amount is the posted delta — what the
            // wallet actually moved — not TransactionAmount, which holds the principal.
            var netByTxn = new Dictionary<int, decimal>();
            decimal credited = 0m, funded = 0m;
            foreach (var row in rows)
            {
                if (!chainFunded.TryGetValue(row.ServiceID, out bool isChainFunded) || !isChainFunded) continue;

                foreach (var tier in ChainLegDescriptions.FundedTiers)
                {
                    bool isDownlineSide = row.TransactionDescription.StartsWith(tier.Credit, StringComparison.Ordinal)
                                          || row.TransactionDescription.StartsWith(tier.CreditUndo, StringComparison.Ordinal);
                    bool isAdminSide = row.TransactionDescription.StartsWith(tier.Funded, StringComparison.Ordinal)
                                       || row.TransactionDescription.StartsWith(tier.FundedUndo, StringComparison.Ordinal);
                    if (!isDownlineSide && !isAdminSide) continue;

                    netByTxn[row.ReferenceID] = netByTxn.GetValueOrDefault(row.ReferenceID) + row.Delta;
                    if (isDownlineSide) credited += row.Delta; else funded += row.Delta;
                    break;
                }
            }

            var broken = netByTxn.Where(x => Math.Abs(x.Value) > Tolerance)
                                 .OrderBy(x => x.Key)
                                 .ToList();
            decimal unbalanced = broken.Sum(x => x.Value);

            string detail = broken.Count == 0
                ? "Every downline commission on a chain-funded service is matched by the funding leg that paid for it."
                : $"{broken.Count} transaction(s) where the commission legs do not sum to zero — a commission nobody funded, or a funding reversal with no funding behind it. "
                  + $"Txn {string.Join(", ", broken.Take(10).Select(x => $"#{x.Key} ({x.Value:+0.00;-0.00})"))}"
                  + (broken.Count > 10 ? $" and {broken.Count - 10} more." : ".");

            // The two totals will not quite differ BY the figure reported, and saying nothing about
            // that is the quickest way to lose an accountant's trust in the whole check. A slab
            // commission is held to more decimals than a wallet is, so the credit and the debit of a
            // balanced pair can land a fraction of a paisa apart. Netted per transaction that is
            // invisible, which is why the reported difference counts only transactions out by more
            // than a paisa; summed across hundreds of them it is a figure you can see. Name it.
            decimal spread = netByTxn.Values.Sum() - unbalanced;
            int spreadTxns = netByTxn.Count(x => Math.Abs(x.Value) > 0.0001m && Math.Abs(x.Value) <= Tolerance);
            if (spreadTxns > 0)
                detail += $" The two totals differ by a further Rs {spread:0.00} of rounding spread over {spreadTxns} transaction(s),"
                          + " none of them out by as much as a paisa — a commission is held to more decimal places than a wallet is.";

            return new IntegrityCheckResult
            {
                Check = "Chain commissions funded",
                LedgerFigure = Math.Round(credited, 2),
                OperationalFigure = Math.Round(funded, 2),
                LedgerLabel = "downline legs",
                OperationalLabel = "funding legs",
                Difference = Math.Round(unbalanced, 2),
                Ok = Math.Abs(unbalanced) <= Tolerance && broken.Count == 0,
                Detail = detail,
            };
        }

        public async Task<IntegrityReport> RunAll()
        {
            var tb = await TrialBalance(string.Empty);
            var checks = new List<IntegrityCheckResult>
            {
                new()
                {
                    Check = "Trial balance",
                    LedgerFigure = tb.TotalDebit,
                    OperationalFigure = tb.TotalCredit,
                    Difference = Math.Round(tb.TotalDebit - tb.TotalCredit, 2),
                    Ok = tb.Balanced,
                    Detail = "Total debits against total credits across every posted voucher.",
                },
                await WalletVsControl(),
                await ProviderStockVsControl(),
                await ChainCommissionsFunded(),
            };
            return new IntegrityReport
            {
                RanAt = DateTime.UtcNow,
                AllOk = checks.All(c => c.Ok),
                Checks = checks,
            };
        }

        /// <summary>A control account's balance on its normal side, from posted voucher lines.</summary>
        private async Task<decimal> ControlBalance(string code, string normalSide)
        {
            int acctId = await _context.AcctAccounts.AsNoTracking()
                .Where(a => a.Code == code && !a.IsDeleted).Select(a => a.AcctAccountID).FirstOrDefaultAsync();
            if (acctId == 0) return 0m;

            var postedVoucherIds = _context.AcctVouchers.AsNoTracking()
                .Where(v => !v.IsDeleted && v.Status == VoucherStatus.Posted).Select(v => v.AcctVoucherID);

            var totals = await _context.AcctVoucherLines.AsNoTracking()
                .Where(l => l.AcctAccountID == acctId && !l.IsDeleted && postedVoucherIds.Contains(l.AcctVoucherID))
                .GroupBy(l => 1)
                .Select(g => new { Dr = g.Sum(x => x.Debit), Cr = g.Sum(x => x.Credit) })
                .FirstOrDefaultAsync();
            if (totals == null) return 0m;
            return normalSide == AccountSide.Debit ? totals.Dr - totals.Cr : totals.Cr - totals.Dr;
        }
    }
}
