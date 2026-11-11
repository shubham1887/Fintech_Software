using FintechApp.Entities;
using FintechApp.Requests.Phase4;
using FintechApp.Responses.Phase4;

namespace FintechApp.Interfaces.Services
{
    /// <summary>The closed-period gate. A locked period rejects every posting into it.</summary>
    public interface IPeriodLockService
    {
        Task<bool> IsLocked(string periodKey);
        Task<string> Lock(PeriodLockRequest request);
        Task<string> Unlock(PeriodLockRequest request);
        Task<List<AcctPeriodLock>> GetLocks();
    }

    /// <summary>The chart of accounts and account balances (computed from posted voucher lines).</summary>
    public interface IChartOfAccountsService
    {
        Task<List<AcctAccount>> GetAccounts();
    }

    /// <summary>
    /// The posting engine. Events are enqueued, then turned into balanced vouchers by the seeded
    /// rules — in the outbox, so a posting that cannot be made is a visible, retried row rather than
    /// a number quietly left wrong. Manual journals and reversals go through the same balanced-write.
    /// </summary>
    public interface ILedgerPostingService
    {
        Task<EnqueuePostingResult> Enqueue(EnqueuePostingRequest request);
        Task<OutboxProcessResult> ProcessOutbox(int maxItems);
        Task<(PostResult? Result, string Failure)> PostManualJournal(ManualJournalRequest request);
        Task<(PostResult? Result, string Failure)> ReverseVoucher(ReverseVoucherRequest request);

        Task<List<VoucherRow>> GetVouchers(LedgerQueryRequest query);
        Task<VoucherRow?> GetVoucher(int voucherId);
        Task<List<AcctPostingOutbox>> GetOutbox(string statusFilter);
    }

    /// <summary>The daily integrity checks — the point of keeping books next to the wallets.</summary>
    public interface ILedgerIntegrityService
    {
        Task<TrialBalanceResult> TrialBalance(string periodKey);
        Task<IntegrityCheckResult> WalletVsControl();
        Task<IntegrityCheckResult> ProviderStockVsControl();
        Task<IntegrityReport> RunAll();
    }
}
