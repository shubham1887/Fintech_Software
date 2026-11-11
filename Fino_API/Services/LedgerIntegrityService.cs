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
        private const decimal Tolerance = 0.01m;

        public LedgerIntegrityService(ApplicationDbContext context) => _context = context;

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
            // The operational figure: the sum of every user's live wallet balance.
            decimal operational = await _context.UserBalanceMaster.AsNoTracking()
                .Where(b => !b.IsDeleted).SumAsync(b => (decimal?)b.Balance) ?? 0m;

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
