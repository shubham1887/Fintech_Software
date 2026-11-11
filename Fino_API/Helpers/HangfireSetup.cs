using FintechApp.Jobs;
using FintechApp.StaticModels;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;

namespace FintechApp.Helpers
{
    /// <summary>
    /// Hangfire wiring for the automated settlement sweeps.
    ///
    /// The schedule lives in SQL (Hangfire's own tables, created on first run), not in the web
    /// process, so: only one server runs a given tick even with several app instances behind a load
    /// balancer; a tick that overruns is skipped instead of stacking; every run, its duration and
    /// its exception are visible in the dashboard; and the operator can pause, re-run or re-time a
    /// job without a redeploy.
    ///
    /// Everything here is defensive on purpose — if the Hangfire schema cannot be prepared (no DB
    /// yet, a read-only login, a first deploy), the API must still start and serve traffic. The
    /// sweeps are a background nicety; refusing to boot over them would be the worse failure.
    ///
    /// Config (appsettings "StatusCheck"):
    ///   Enabled           — master switch for the recurring jobs (default true)
    ///   IntervalMinutes   — recharge + DMT sweep cadence (default 5)
    ///   MinAgeSeconds     — how old a pending txn must be before it is probed (default 90)
    ///   BatchSize         — max transactions probed per run (default 50)
    ///   MaxParallel       — concurrent provider probes within one run (default 4)
    ///   DashboardEnabled  — expose /hangfire (default: development only)
    /// </summary>
    public static class HangfireSetup
    {
        public const string DashboardPath = "/hangfire";

        /// <summary>
        /// Every cron here is written in the SERVER's clock ("23:50" means 23:50 on this box). Hangfire
        /// reads a cron in UTC unless told otherwise, which put the 23:50 nightly close at 05:20 the
        /// next morning on an IST server and the 9am reminder at 2:30pm.
        /// </summary>
        private static readonly RecurringJobOptions LocalTime = new() { TimeZone = TimeZoneInfo.Local };

        public static IServiceCollection AddFintechHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(connectionString)) return services;

            services.AddHangfire(cfg => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    // The defaults are tuned for a dedicated job server; these keep the polling
                    // footprint small on the same SQL instance the API already uses.
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true,
                }));

            // A handful of workers is plenty: the sweeps are I/O-bound and already throttle their
            // own provider calls with StatusCheck:MaxParallel.
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Math.Max(configuration.GetValue("StatusCheck:Workers", 4), 1);
                options.ServerName = $"fintech-{Environment.MachineName}";
            });

            services.AddScoped<TransactionStatusCheckJob>();
            services.AddScoped<GstMaintenanceJob>();
            services.AddScoped<AccountingJob>();
            services.AddScoped<OutstandingReminderJob>();
            services.AddScoped<MonthlyInvoiceJob>();
            services.AddScoped<DailySnapshotJob>();
            return services;
        }

        /// <summary>
        /// Registers the recurring jobs and (optionally) mounts the dashboard. Safe to call when
        /// Hangfire was never registered or the storage is unreachable — it logs and returns.
        /// </summary>
        public static void UseFintechHangfire(this WebApplication app)
        {
            var log = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Hangfire");
            var cfg = app.Configuration.GetSection("StatusCheck");

            if (app.Services.GetService<JobStorage>() == null)
            {
                log.LogWarning("Hangfire is not configured (no connection string) — automated status checks are OFF.");
                return;
            }

            bool dashboard = app.Configuration.GetValue("StatusCheck:DashboardEnabled", app.Environment.IsDevelopment());
            if (dashboard)
            {
                app.UseHangfireDashboard(DashboardPath, new DashboardOptions
                {
                    Authorization = new[] { new AdminOnlyDashboardFilter(app.Environment.IsDevelopment()) },
                    DashboardTitle = "Fintech background jobs",
                    // The job list is operational data, not something to hand to a search engine.
                    DisplayStorageConnectionString = false,
                });
            }

            // GST background sweeps. Registered independently of the settlement StatusCheck switch
            // below, in their own try/catch, so neither feature's config can turn the other off and
            // a failure to register one never stops the API booting.
            try
            {
                var e = app.Configuration.GetSection(EInvoiceSettings.Section).Get<EInvoiceSettings>() ?? new EInvoiceSettings();
                if (e.ScheduleEnabled)
                {
                    // The queue is worked only where a portal exists; otherwise the job would wake
                    // every interval to do nothing. The two alert sweeps run regardless — a deadline
                    // counts down whether or not a gateway is configured, and matters most when one
                    // is not.
                    if (e.IsConfigured)
                    {
                        int mins = e.ProcessIntervalMinutes < 1 ? 30 : e.ProcessIntervalMinutes;
                        string queueCron = mins >= 60 ? Cron.Hourly() : $"*/{mins} * * * *";
                        RecurringJob.AddOrUpdate<GstMaintenanceJob>(
                            GstMaintenanceJob.ProcessQueueJobId, j => j.ProcessQueue(), queueCron, LocalTime);
                    }
                    else
                    {
                        TryRemove(GstMaintenanceJob.ProcessQueueJobId, log);
                    }

                    string alertCron = string.IsNullOrWhiteSpace(e.AlertCron) ? "0 7 * * *" : e.AlertCron;
                    RecurringJob.AddOrUpdate<GstMaintenanceJob>(
                        GstMaintenanceJob.DeadlineAlertJobId, j => j.SweepDeadlineAlerts(), alertCron, LocalTime);
                    RecurringJob.AddOrUpdate<GstMaintenanceJob>(
                        GstMaintenanceJob.CreditNoteAlertJobId, j => j.SweepCreditNoteWindowAlerts(), alertCron, LocalTime);

                    log.LogInformation(
                        "GST sweeps registered: IRN queue {QueueState}; deadline + credit-note alerts at '{Cron}'.",
                        e.IsConfigured ? $"every {e.ProcessIntervalMinutes} min" : "OFF (no gateway configured)", alertCron);
                }
                else
                {
                    TryRemove(GstMaintenanceJob.ProcessQueueJobId, log);
                    TryRemove(GstMaintenanceJob.DeadlineAlertJobId, log);
                    TryRemove(GstMaintenanceJob.CreditNoteAlertJobId, log);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Could not register the GST Hangfire jobs; they will retry on the next restart.");
            }

            // Month-end invoicing: issue our OWN sale documents for the month that just ended.
            //
            // Scheduled rather than left to a button, because a GST month closes whether or not
            // anybody opens a screen — and on an install sold white-label, "somebody remembers to
            // press Generate every month" is not a plan. Safe to run repeatedly: generation skips
            // every source already carried by a document, and it refuses outright when a party due
            // a document has no place of supply rather than issuing one the portal will reject.
            //
            // Config "Invoicing": ScheduleEnabled (default true), Cron (default 02:30 on the 2nd
            // of each month — the 2nd, not the 1st, so a late settlement on the last day of the
            // month is already in the books when the month is invoiced).
            try
            {
                var inv = app.Configuration.GetSection("Invoicing");
                if (inv.GetValue("ScheduleEnabled", true))
                {
                    string invoiceCron = inv.GetValue("Cron", "30 2 2 * *") ?? "30 2 2 * *";
                    RecurringJob.AddOrUpdate<MonthlyInvoiceJob>(
                        MonthlyInvoiceJob.JobId, j => j.Run(), invoiceCron, LocalTime);
                    log.LogInformation("Month-end invoicing registered at '{Cron}'.", invoiceCron);
                }
                else
                {
                    TryRemove(MonthlyInvoiceJob.JobId, log);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Could not register the month-end invoicing job; it will retry on the next restart.");
            }

            // Ledger sweeps (Phase 4): work the posting outbox often, run the integrity checks daily.
            // Own try/catch, independent of the switches above. Config "Accounting":
            //   ScheduleEnabled (default true), OutboxIntervalMinutes (default 10), IntegrityCron (default 6am).
            try
            {
                var acct = app.Configuration.GetSection("Accounting");
                if (acct.GetValue("ScheduleEnabled", true))
                {
                    int mins = acct.GetValue("OutboxIntervalMinutes", 10);
                    if (mins < 1) mins = 10;
                    string outboxCron = mins >= 60 ? Cron.Hourly() : $"*/{mins} * * * *";
                    RecurringJob.AddOrUpdate<AccountingJob>(
                        AccountingJob.ProcessOutboxJobId, j => j.ProcessOutbox(), outboxCron, LocalTime);

                    string integrityCron = acct.GetValue("IntegrityCron", "0 6 * * *") ?? "0 6 * * *";
                    RecurringJob.AddOrUpdate<AccountingJob>(
                        AccountingJob.IntegrityJobId, j => j.RunIntegrityChecks(), integrityCron, LocalTime);

                    log.LogInformation("Ledger sweeps registered: outbox every {Mins} min, integrity checks at '{Cron}'.", mins, integrityCron);
                }
                else
                {
                    TryRemove(AccountingJob.ProcessOutboxJobId, log);
                    TryRemove(AccountingJob.IntegrityJobId, log);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Could not register the ledger Hangfire jobs; they will retry on the next restart.");
            }

            // The morning outstanding reminder. Its own try/catch and its own switch, like the
            // sweeps above, so neither feature's configuration can turn the other off and a
            // failure to register it never stops the API booting.
            //
            // Config "CashCollection":
            //   ReminderEnabled (default true), ReminderCron (default "0 9 * * *" - 9am server time).
            // The hour is config rather than code because "morning" is local: an install serving
            // one state wants it before the counter opens, and that is not the same clock time
            // everywhere this product is sold.
            try
            {
                var cc = app.Configuration.GetSection("CashCollection");
                if (cc.GetValue("ReminderEnabled", true))
                {
                    string reminderCron = cc.GetValue("ReminderCron", "0 9 * * *") ?? "0 9 * * *";
                    RecurringJob.AddOrUpdate<OutstandingReminderJob>(
                        OutstandingReminderJob.DailyReminderJobId, j => j.SendDailyReminders(), reminderCron, LocalTime);
                    log.LogInformation("Outstanding reminder registered at '{Cron}'.", reminderCron);
                }
                else
                {
                    TryRemove(OutstandingReminderJob.DailyReminderJobId, log);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Could not register the outstanding reminder job; it will retry on the next restart.");
            }

            // The end-of-day close: backup, provider balance enquiry, and the closing snapshot of
            // every user wallet and every provider — inside the nightly maintenance window.
            // Config "NightlySnapshot": Enabled (default true), Cron (default 23:50), BackupFolder,
            // KeepDays (default 7).
            try
            {
                var ns = app.Configuration.GetSection("NightlySnapshot");
                if (ns.GetValue("Enabled", true))
                {
                    string snapshotCron = ns.GetValue("Cron", "50 23 * * *") ?? "50 23 * * *";
                    RecurringJob.AddOrUpdate<DailySnapshotJob>(DailySnapshotJob.JobId, j => j.Run(), snapshotCron, LocalTime);
                    log.LogInformation("Nightly snapshot (backup + balances) registered at '{Cron}'.", snapshotCron);
                }
                else
                {
                    TryRemove(DailySnapshotJob.JobId, log);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Could not register the nightly snapshot job; it will retry on the next restart.");
            }

            if (!cfg.GetValue("Enabled", true))
            {
                log.LogInformation("Automated status checks are disabled (StatusCheck:Enabled=false). Removing recurring jobs.");
                TryRemove(TransactionStatusCheckJob.RechargeJobId, log);
                TryRemove(TransactionStatusCheckJob.DmtJobId, log);
                TryRemove(TransactionStatusCheckJob.PayoutJobId, log);
                TryRemove(TransactionStatusCheckJob.DmtReleaseJobId, log);
                return;
            }

            int minutes = cfg.GetValue("IntervalMinutes", 5);
            if (minutes < 1) minutes = 5;
            string sweepCron = minutes >= 60 ? Cron.Hourly() : $"*/{minutes} * * * *";

            try
            {
                RecurringJob.AddOrUpdate<TransactionStatusCheckJob>(
                    TransactionStatusCheckJob.RechargeJobId, j => j.SweepRecharge(), sweepCron, LocalTime);
                RecurringJob.AddOrUpdate<TransactionStatusCheckJob>(
                    TransactionStatusCheckJob.DmtJobId, j => j.SweepDmt(), sweepCron, LocalTime);
                // Payout is its own service family, so the DMT sweep never saw it. An asynchronous
                // payout rail answers "pending" to every initiation, which makes this the only
                // automated route to a final status when a webhook is missed.
                RecurringJob.AddOrUpdate<TransactionStatusCheckJob>(
                    TransactionStatusCheckJob.PayoutJobId, j => j.SweepPayout(), sweepCron, LocalTime);
                // Safety net only — turning the hold off enqueues this immediately. Hourly is enough
                // to catch a release that was lost to a restart mid-enqueue.
                RecurringJob.AddOrUpdate<TransactionStatusCheckJob>(
                    TransactionStatusCheckJob.DmtReleaseJobId, j => j.ReleaseHeldDmt(), Cron.Hourly(), LocalTime);

                log.LogInformation(
                    "Hangfire recurring jobs registered: recharge + DMT + payout status check every {Minutes} min, held-DMT release hourly.{Dash}",
                    minutes, dashboard ? $" Dashboard at {DashboardPath}." : string.Empty);
            }
            catch (Exception ex)
            {
                // Storage unreachable at boot — the API still serves traffic; the jobs will be
                // registered on the next successful start.
                log.LogError(ex, "Could not register the Hangfire recurring jobs. Automated status checks are OFF until the next restart.");
            }
        }

        private static void TryRemove(string jobId, ILogger log)
        {
            try { RecurringJob.RemoveIfExists(jobId); }
            catch (Exception ex) { log.LogWarning(ex, "Could not remove recurring job {JobId}.", jobId); }
        }
    }

    /// <summary>
    /// Dashboard access rule. The dashboard exposes job arguments (transaction ids) and lets a
    /// visitor trigger or delete jobs, so it is never open: local requests only, unless the host is
    /// explicitly running in development. Remote operators reach the same controls through the
    /// admin panel's own authenticated endpoints instead.
    /// </summary>
    public class AdminOnlyDashboardFilter : IDashboardAuthorizationFilter
    {
        private readonly bool _allowAnyInDevelopment;

        public AdminOnlyDashboardFilter(bool allowAnyInDevelopment) => _allowAnyInDevelopment = allowAnyInDevelopment;

        public bool Authorize(DashboardContext context)
        {
            var http = context.GetHttpContext();
            if (_allowAnyInDevelopment) return true;

            var remote = http.Connection.RemoteIpAddress;
            return remote != null && System.Net.IPAddress.IsLoopback(remote);
        }
    }
}
