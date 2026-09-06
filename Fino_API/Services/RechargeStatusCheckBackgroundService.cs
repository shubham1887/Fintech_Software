using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    // Automated pending-recharge status checker.
    //
    // Why this exists: a recharge whose provider replies PROCESS/PENDING is deliberately left in
    // PENDING by RechargeService — "settled later via status-check or manual update". Nothing was
    // settling it, so recharges sat in PROCESS forever. The old FinovativeAPI ran this as a Hangfire
    // recurring job ("Txn Status Check", StatusCheckProcessing.StatusCheck(), every StatusCheckInterval
    // minutes). This is the same job as a hosted background service — no extra dependency.
    //
    // Each tick pulls the oldest pending recharge-family transactions that have already been dispatched
    // to a provider and are old enough to have settled, then re-enquires each through the existing
    // ITransactionProcessingService.CheckProviderStatus pipeline (provider STATUS endpoint → SUCCESS
    // finalises, FAILED refunds, still-PENDING is left for the next tick). Providers reject duplicate
    // request ids, so a re-enquiry can never double-charge.
    public class RechargeStatusCheckBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RechargeStatusCheckBackgroundService> _log;
        private readonly Logger _fileLogger = new();

        private readonly bool _enabled;
        private readonly TimeSpan _interval;
        private readonly int _minAgeSeconds;
        private readonly int _batchSize;
        private readonly int _maxParallel;

        public RechargeStatusCheckBackgroundService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<RechargeStatusCheckBackgroundService> log)
        {
            _scopeFactory = scopeFactory;
            _log = log;

            var cfg = configuration.GetSection("StatusCheck");
            _enabled = cfg.GetValue("Enabled", true);
            int minutes = cfg.GetValue("IntervalMinutes", 5);
            if (minutes < 1) minutes = 5;
            _interval = TimeSpan.FromMinutes(minutes);
            _minAgeSeconds = cfg.GetValue("MinAgeSeconds", 90);
            _batchSize = cfg.GetValue("BatchSize", 50);
            _maxParallel = Math.Max(cfg.GetValue("MaxParallel", 4), 1);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_enabled)
            {
                _log.LogInformation("Recharge status-check scheduler is disabled (StatusCheck:Enabled=false).");
                return;
            }

            _log.LogInformation("Recharge status-check scheduler started: every {Interval} min, batch {Batch}, min age {Age}s.",
                _interval.TotalMinutes, _batchSize, _minAgeSeconds);

            // Small startup delay so the app finishes warming up before the first sweep.
            try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
            catch (OperationCanceledException) { return; }

            using var timer = new PeriodicTimer(_interval);
            do
            {
                try
                {
                    await RunOnce(stoppingToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Recharge status-check sweep failed.");
                    _fileLogger.Write(ex);
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task RunOnce(CancellationToken ct)
        {
            List<Entities.TransactionMaster> pending;
            using (var scope = _scopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ITransactionMasterRepository>();
                pending = await repo.GetPendingRechargeForStatusCheck(_minAgeSeconds, _batchSize);
            }

            // Log the empty case too — it tells the operator the sweep DID run but nothing was
            // eligible (vs. the scheduler not running at all), which is the first thing to rule out
            // when "status-check is not working". Nothing eligible usually means: no recharge is
            // PENDING/PROCESS, they are younger than MinAgeSeconds, or they carry ProviderID = 0.
            if (pending.Count == 0)
            {
                _log.LogInformation(
                    "Recharge status-check: ran, 0 eligible pending recharge(s) (status PENDING/PROCESS, ProviderID>0, older than {Age}s).",
                    _minAgeSeconds);
                return;
            }
            _log.LogInformation("Recharge status-check: probing {Count} pending recharge transaction(s).", pending.Count);

            int settled = 0, stillPending = 0, errored = 0;
            using var throttler = new SemaphoreSlim(_maxParallel);
            var tasks = pending.Select(async txn =>
            {
                await throttler.WaitAsync(ct);
                try
                {
                    // Fresh scope per transaction — EF Core DbContext is not thread-safe, so
                    // concurrent probes must not share one scoped context.
                    using var scope = _scopeFactory.CreateScope();
                    var processing = scope.ServiceProvider.GetRequiredService<ITransactionProcessingService>();
                    var repo = scope.ServiceProvider.GetRequiredService<ITransactionMasterRepository>();

                    string reply = await processing.CheckProviderStatus(
                        txn.TransactionMasterID, ServiceKinds.Recharge, txn.UserID, "SCHEDULER");

                    // Re-read the row to see whether this probe actually settled it. A probe that
                    // leaves the transaction PENDING is the silent failure we most need to surface:
                    // the provider reply (e.g. "Service Unavailable") tells us WHY it did nothing —
                    // typically the provider's STATUS endpoint / STATUS response-rules aren't configured.
                    var after = await repo.GetByID(txn.TransactionMasterID);
                    string newStatus = after?.Status ?? "?";
                    bool resolved = !newStatus.Equals("PENDING", StringComparison.OrdinalIgnoreCase)
                                 && !newStatus.Equals("PROCESS", StringComparison.OrdinalIgnoreCase);

                    if (resolved)
                    {
                        Interlocked.Increment(ref settled);
                        _log.LogInformation(
                            "Recharge status-check: txn {Id} (provider {Pid}) settled -> {Status}. Reply: {Reply}",
                            txn.TransactionMasterID, txn.ProviderID, newStatus, reply);
                    }
                    else
                    {
                        Interlocked.Increment(ref stillPending);
                        _log.LogWarning(
                            "Recharge status-check: txn {Id} (provider {Pid}) STILL PENDING after probe. Provider reply: {Reply}",
                            txn.TransactionMasterID, txn.ProviderID, reply);
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref errored);
                    _log.LogError(ex, "Recharge status-check failed for transaction {Id}.", txn.TransactionMasterID);
                    _fileLogger.Write(ex);
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);

            _log.LogInformation(
                "Recharge status-check tick complete: {Total} probed, {Settled} settled, {Pending} still pending, {Errored} errored.",
                pending.Count, settled, stillPending, errored);
        }
    }
}
