using FintechApp.Helpers;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;
using Hangfire;
using Microsoft.Extensions.Options;

namespace FintechApp.Jobs
{
    /// <summary>
    /// The GST background sweeps, run by Hangfire alongside the settlement ones.
    ///
    /// Three jobs, because three things count down on their own and were previously discovered only
    /// when someone thought to open a screen:
    ///   - the IRN queue has to be worked, or documents sit Pending while their 30-day window runs;
    ///   - that 30-day e-invoice deadline needs an alert while something can still be done;
    ///   - the credit-note window (30 November) needs the same, for a reversal that is still free.
    ///
    /// Each resolves its own scope per run (a recurring job outlives any request scope) and is
    /// marked DisableConcurrentExecution so a slow run never overlaps the next. The alert sweeps
    /// write to the log at Warning when anything is near or past a deadline — visible in the
    /// Hangfire dashboard and wherever logs are shipped — rather than taking an irreversible action
    /// on their own. Processing the queue is the one that acts, and it no-ops when no gateway is
    /// configured, so it is only scheduled on an install that has one.
    /// </summary>
    public class GstMaintenanceJob
    {
        public const string ProcessQueueJobId = "einvoice-process-queue";
        public const string DeadlineAlertJobId = "einvoice-deadline-alerts";
        public const string CreditNoteAlertJobId = "creditnote-window-alerts";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly EInvoiceSettings _cfg;
        private readonly ILogger<GstMaintenanceJob> _log;

        public GstMaintenanceJob(
            IServiceScopeFactory scopeFactory,
            IOptions<EInvoiceSettings> cfg,
            ILogger<GstMaintenanceJob> log)
        {
            _scopeFactory = scopeFactory;
            _cfg = cfg.Value;
            _log = log;
        }

        /// <summary>Work the IRN queue, soonest-deadline-first. No-op without a configured gateway.</summary>
        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        public async Task ProcessQueue()
        {
            using var scope = _scopeFactory.CreateScope();
            var einvoice = scope.ServiceProvider.GetRequiredService<IEInvoiceService>();
            var gateway = scope.ServiceProvider.GetRequiredService<IEInvoiceGateway>();

            if (!gateway.IsConfigured)
            {
                _log.LogInformation("e-invoice: queue processing skipped — no IRP gateway configured.");
                return;
            }

            var result = await einvoice.ProcessQueue(_cfg.ProcessBatchSize);
            _log.LogInformation(
                "e-invoice queue: attempted {Attempted}, registered {Registered}, retryable {Retry}, permanent {Perm}, past-deadline {Past}.",
                result.Attempted, result.Registered, result.FailedRetryable, result.FailedPermanent, result.PastDeadline);

            if (result.FailedPermanent > 0 || result.PastDeadline > 0)
                _log.LogWarning(
                    "e-invoice queue: {Perm} document(s) rejected on their merits and {Past} past the 30-day limit — these need a person.",
                    result.FailedPermanent, result.PastDeadline);
        }

        /// <summary>Surface the documents whose IRN deadline is near or past.</summary>
        [DisableConcurrentExecution(timeoutInSeconds: 120)]
        public async Task SweepDeadlineAlerts()
        {
            using var scope = _scopeFactory.CreateScope();
            var einvoice = scope.ServiceProvider.GetRequiredService<IEInvoiceService>();

            var alerts = await einvoice.GetDeadlineAlerts(DateTime.Today, _cfg.AlertDaysAhead);
            int overdue = alerts.Count(a => a.DeadlinePassed);
            if (alerts.Count == 0)
            {
                _log.LogInformation("e-invoice deadlines: nothing within {Days} days.", _cfg.AlertDaysAhead);
                return;
            }
            _log.LogWarning(
                "e-invoice deadlines: {Total} document(s) within {Days} days of the 30-day limit, {Overdue} already past it (no IRN obtainable).",
                alerts.Count, _cfg.AlertDaysAhead, overdue);
        }

        /// <summary>Surface the issued documents whose credit-note window is closing while a source on them failed.</summary>
        [DisableConcurrentExecution(timeoutInSeconds: 120)]
        public async Task SweepCreditNoteWindowAlerts()
        {
            using var scope = _scopeFactory.CreateScope();
            var notes = scope.ServiceProvider.GetRequiredService<ICreditNoteService>();

            var alerts = await notes.GetWindowAlerts(DateTime.Today, _cfg.AlertDaysAhead);
            int closed = alerts.Count(a => a.WindowClosed);
            if (alerts.Count == 0)
            {
                _log.LogInformation("credit-note windows: nothing closing within {Days} days.", _cfg.AlertDaysAhead);
                return;
            }
            _log.LogWarning(
                "credit-note windows: {Total} document(s) closing within {Days} days, {Closed} already closed (a note now carries no tax).",
                alerts.Count, _cfg.AlertDaysAhead, closed);
        }
    }
}
