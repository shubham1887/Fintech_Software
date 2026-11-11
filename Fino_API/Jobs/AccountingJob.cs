using FintechApp.Interfaces.Services;
using Hangfire;

namespace FintechApp.Jobs
{
    /// <summary>
    /// The ledger's background jobs.
    ///
    /// The outbox is worked often, because a posting that is queued but never made is the thing the
    /// outbox exists to prevent — a short cadence keeps the books close to the events. The integrity
    /// checks run once a day and shout (log at Warning) the moment the ledger stops agreeing with
    /// the wallets or the provider stock, so a drift is caught on the day it starts rather than at a
    /// year-end that does not reconcile.
    /// </summary>
    public class AccountingJob
    {
        public const string ProcessOutboxJobId = "ledger-process-outbox";
        public const string IntegrityJobId = "ledger-integrity-check";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AccountingJob> _log;

        public AccountingJob(IServiceScopeFactory scopeFactory, ILogger<AccountingJob> log)
        {
            _scopeFactory = scopeFactory;
            _log = log;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        public async Task ProcessOutbox()
        {
            using var scope = _scopeFactory.CreateScope();
            var ledger = scope.ServiceProvider.GetRequiredService<ILedgerPostingService>();

            var r = await ledger.ProcessOutbox(500);
            if (r.Attempted == 0) return;
            _log.LogInformation("ledger outbox: attempted {A}, posted {P}, failed {F}, skipped {S}.",
                r.Attempted, r.Posted, r.Failed, r.Skipped);
            if (r.Failed > 0)
                _log.LogWarning("ledger outbox: {F} event(s) could not post — see AcctPostingOutbox.LastError. {Msgs}",
                    r.Failed, string.Join(" | ", r.Messages.Take(10)));
        }

        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        public async Task RunIntegrityChecks()
        {
            using var scope = _scopeFactory.CreateScope();
            var integrity = scope.ServiceProvider.GetRequiredService<ILedgerIntegrityService>();

            var report = await integrity.RunAll();
            if (report.AllOk)
            {
                _log.LogInformation("ledger integrity: all {N} checks passed.", report.Checks.Count);
                return;
            }
            foreach (var c in report.Checks.Where(c => !c.Ok))
                _log.LogWarning("ledger integrity FAILED: {Check} — ledger {Ledger:0.00} vs operational {Op:0.00}, diff {Diff:0.00}. {Detail}",
                    c.Check, c.LedgerFigure, c.OperationalFigure, c.Difference, c.Detail);
        }
    }
}
