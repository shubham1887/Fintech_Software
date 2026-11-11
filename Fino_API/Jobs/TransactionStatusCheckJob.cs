using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;
using Hangfire;

namespace FintechApp.Jobs
{
    /// <summary>
    /// Automated settlement sweeps, run by Hangfire.
    ///
    /// Why Hangfire and not a hosted BackgroundService: the old sweeper woke every 5 minutes inside
    /// the web process with nothing coordinating it. Two app instances (or one recycled mid-sweep)
    /// ran the same probe set twice, a slow provider made a tick overlap the next one, and the work
    /// was invisible — no history, no retry, no way to pause it. Hangfire keeps the schedule in SQL,
    /// so exactly one server runs a given tick (DisableConcurrentExecution + the distributed lock in
    /// its storage), every run is recorded in the dashboard, and a failed sweep is visible rather
    /// than silent.
    ///
    /// Each sweep pulls the oldest eligible pending transactions of one service family and
    /// re-enquires each through ITransactionProcessingService.CheckProviderStatus (provider STATUS
    /// endpoint → SUCCESS finalises, FAILED refunds, still-pending is left for the next run).
    /// Providers reject duplicate request ids, so a re-enquiry can never double-charge.
    /// </summary>
    public class TransactionStatusCheckJob
    {
        // Hangfire recurring-job ids — also used by the admin screen to trigger a run on demand.
        public const string RechargeJobId = "txn-status-check-recharge";
        public const string DmtJobId = "txn-status-check-dmt";
        public const string PayoutJobId = "txn-status-check-payout";
        public const string DmtReleaseJobId = "dmt-release-held";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TransactionStatusCheckJob> _log;
        private readonly IConfiguration _configuration;
        private readonly Logger _fileLogger = new();

        public TransactionStatusCheckJob(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<TransactionStatusCheckJob> log)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _log = log;
        }

        private (int minAge, int batch, int parallel) Tuning()
        {
            var cfg = _configuration.GetSection("StatusCheck");
            return (
                cfg.GetValue("MinAgeSeconds", 90),
                cfg.GetValue("BatchSize", 50),
                Math.Max(cfg.GetValue("MaxParallel", 4), 1));
        }

        /// <summary>Recharge-family sweep (prepaid / postpaid / DTH …).</summary>
        // One run at a time across the whole farm; a run still going when the next tick fires is
        // skipped rather than queued, which is what keeps a slow provider from piling up work.
        [DisableConcurrentExecution(timeoutInSeconds: 0)]
        [AutomaticRetry(Attempts = 0)]
        public Task SweepRecharge() => Sweep(ServiceKinds.Recharge);

        /// <summary>Money-transfer sweep — same settlement path, DMT service family.</summary>
        [DisableConcurrentExecution(timeoutInSeconds: 0)]
        [AutomaticRetry(Attempts = 0)]
        public Task SweepDmt() => Sweep(ServiceKinds.DMT);

        /// <summary>
        /// Payout sweep — same settlement path, PAYOUT service family.
        ///
        /// The sweep is per service family and a service the admin marked PAYOUT is not in the DMT
        /// family, so without this a payout had no automated enquiry at all: its only route to a
        /// final status was the provider's webhook. That is a thin thread to hang settlement on —
        /// a missed push, a webhook URL not yet configured, a few minutes of our site being down,
        /// and the transfer sits PENDING until someone settles it by hand. It matters more with an
        /// asynchronous payout rail (Nexmint answers "pending" to EVERY initiation) than it ever
        /// did with a gateway that answered synchronously.
        /// </summary>
        [DisableConcurrentExecution(timeoutInSeconds: 0)]
        [AutomaticRetry(Attempts = 0)]
        public Task SweepPayout() => Sweep(ServiceKinds.Payout);

        private async Task Sweep(string serviceKind)
        {
            var (minAge, batch, parallel) = Tuning();

            List<Entities.TransactionMaster> pending;
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ITransactionMasterRepository>();
                pending = await repo.GetPendingForStatusCheck(serviceKind, minAge, batch);
            }

            // Log the empty case too — it tells the operator the sweep DID run but nothing was
            // eligible (vs. the scheduler not running at all), which is the first thing to rule out
            // when "status-check is not working". Nothing eligible usually means: nothing is
            // PENDING/PROCESS, the rows are younger than MinAgeSeconds, or they carry ProviderID = 0.
            if (pending.Count == 0)
            {
                _log.LogInformation(
                    "{Kind} status-check: ran, 0 eligible pending transaction(s) (PENDING/PROCESS, ProviderID>0, older than {Age}s).",
                    serviceKind, minAge);
                return;
            }
            _log.LogInformation("{Kind} status-check: probing {Count} pending transaction(s).", serviceKind, pending.Count);

            int settled = 0, stillPending = 0, errored = 0;
            using var throttler = new SemaphoreSlim(parallel);
            var tasks = pending.Select(async txn =>
            {
                await throttler.WaitAsync();
                try
                {
                    // Fresh scope per transaction — EF Core DbContext is not thread-safe, so
                    // concurrent probes must not share one scoped context.
                    using var scope = _scopeFactory.CreateScope();
                    var processing = scope.ServiceProvider.GetRequiredService<ITransactionProcessingService>();
                    var repo = scope.ServiceProvider.GetRequiredService<ITransactionMasterRepository>();

                    string reply = await processing.CheckProviderStatus(
                        txn.TransactionMasterID, serviceKind, txn.UserID, "SCHEDULER");

                    // Re-read the row to see whether this probe actually settled it. A probe that
                    // leaves the transaction PENDING is the silent failure we most need to surface:
                    // the provider reply (e.g. "Service Unavailable") tells us WHY it did nothing —
                    // typically the provider STATUS endpoint / response-rules are not configured.
                    var after = await repo.GetByID(txn.TransactionMasterID);
                    string newStatus = after?.Status ?? "?";
                    bool resolved = !newStatus.Equals("PENDING", StringComparison.OrdinalIgnoreCase)
                                 && !newStatus.Equals("PROCESS", StringComparison.OrdinalIgnoreCase);

                    if (resolved)
                    {
                        Interlocked.Increment(ref settled);
                        _log.LogInformation(
                            "{Kind} status-check: txn {Id} (provider {Pid}) settled -> {Status}. Reply: {Reply}",
                            serviceKind, txn.TransactionMasterID, txn.ProviderID, newStatus, reply);
                    }
                    else
                    {
                        Interlocked.Increment(ref stillPending);
                        _log.LogWarning(
                            "{Kind} status-check: txn {Id} (provider {Pid}) STILL PENDING after probe. Provider reply: {Reply}",
                            serviceKind, txn.TransactionMasterID, txn.ProviderID, reply);
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errored);
                    _log.LogError(ex, "{Kind} status-check failed for transaction {Id}.", serviceKind, txn.TransactionMasterID);
                    _fileLogger.Write(ex);
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);

            _log.LogInformation(
                "{Kind} status-check run complete: {Total} probed, {Settled} settled, {Pending} still pending, {Errored} errored.",
                serviceKind, pending.Count, settled, stillPending, errored);
        }

        /// <summary>
        /// Release every money-transfer parked by the global "Hold DMT" switch through the normal
        /// DMT provider sequence. Enqueued the moment an admin turns the hold OFF, and also run on a
        /// slow recurring schedule so a transfer can never stay parked because one enqueue was lost.
        /// Re-entrant-safe: a released transfer gets a ProviderID, which takes it out of the held
        /// set, and the provider's own duplicate-request-id check backs that up.
        /// </summary>
        [DisableConcurrentExecution(timeoutInSeconds: 0)]
        [AutomaticRetry(Attempts = 1)]
        public async Task ReleaseHeldDmt()
        {
            // A hold that is still ON must not be released — the recurring safety net would
            // otherwise undo the switch the admin just set.
            using (var scope = _scopeFactory.CreateScope())
            {
                var settings = scope.ServiceProvider.GetRequiredService<ISettingMasterRepository>();
                var hold = await settings.GetByKey(TransactionHold.DMTHoldSettingKey);
                if (hold != null && hold.IsOn)
                {
                    _log.LogInformation("DMT release: skipped — the DMT hold is still ON.");
                    return;
                }
            }

            List<Entities.TransactionMaster> held;
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ITransactionMasterRepository>();
                held = await repo.GetHeldDMT(200);
            }

            if (held.Count == 0)
            {
                _log.LogInformation("DMT release: ran, nothing is on hold.");
                return;
            }
            _log.LogInformation("DMT release: dispatching {Count} held transfer(s).", held.Count);

            // One at a time, not fanned out: each release is a real money-out call to the bank
            // gateway, and a burst of them at the moment an admin lifts the hold is exactly the
            // traffic spike the hold was protecting against.
            int released = 0, failed = 0;
            foreach (var txn in held)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var transfers = scope.ServiceProvider.GetRequiredService<IBankTransferService>();
                    var (fail, result) = await transfers.ReleaseHeldTransfer(txn.TransactionMasterID, "SCHEDULER");
                    if (!fail.Equals(StatusType.Ok, StringComparison.OrdinalIgnoreCase))
                    {
                        failed++;
                        _log.LogWarning("DMT release: txn {Id} not released — {Reason}", txn.TransactionMasterID, fail);
                        continue;
                    }
                    released++;
                    _log.LogInformation("DMT release: txn {Id} dispatched -> {Status}. {Message}",
                        txn.TransactionMasterID, result?.Status, result?.Message);
                }
                catch (Exception ex)
                {
                    failed++;
                    _log.LogError(ex, "DMT release failed for transaction {Id}.", txn.TransactionMasterID);
                    _fileLogger.Write(ex);
                }
            }

            _log.LogInformation("DMT release complete: {Released} dispatched, {Failed} failed.", released, failed);
        }
    }
}
