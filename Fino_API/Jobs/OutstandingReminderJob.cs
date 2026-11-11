using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using Hangfire;

namespace FintechApp.Jobs
{
    /// <summary>
    /// The morning reminder: everyone still carrying an outstanding balance is told what they owe.
    ///
    /// Why a scheduled sweep rather than a message on each movement — those exist too, and they
    /// are not the same thing. A credit message says "you took ₹50,000 today"; this says "you owe
    /// ₹3,20,000 this morning". A debt that nobody is reminded of is a debt that gets older, and
    /// the whole reason an upline hesitates to extend credit is that chasing it is manual work.
    /// Once a day, before trading starts, is the cadence that makes it routine rather than a
    /// confrontation.
    ///
    /// Deliberate properties:
    ///
    ///  • It only messages balances ABOVE zero. Someone who has cleared their account hears
    ///    nothing, which is what makes the message mean something when it arrives.
    ///
    ///  • It is paced. Sending to every debtor on a large network in one burst is exactly the
    ///    shape of traffic an SMS gateway rate-limits or drops, and a reminder that silently did
    ///    not send is worse than one that took ten minutes to go out.
    ///
    ///  • One failure never stops the sweep. A user with a bad mobile number must not cost
    ///    everybody else their reminder.
    ///
    ///  • It respects the feature switch, so an install that does not use cash collection does
    ///    not text anybody at seven in the morning.
    /// </summary>
    public class OutstandingReminderJob
    {
        public const string DailyReminderJobId = "outstanding-daily-reminder";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutstandingReminderJob> _log;

        /// <summary>
        /// A short pause between messages so a few hundred reminders arrive as a steady stream
        /// rather than a burst the gateway throttles.
        /// </summary>
        private const int PaceMilliseconds = 120;

        public OutstandingReminderJob(IServiceScopeFactory scopeFactory, ILogger<OutstandingReminderJob> log)
        {
            _scopeFactory = scopeFactory;
            _log = log;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 600)]
        public async Task SendDailyReminders()
        {
            // A recurring job outlives any request scope, so it makes its own.
            using var scope = _scopeFactory.CreateScope();
            var outstanding = scope.ServiceProvider.GetRequiredService<IOutstandingService>();

            if (!await outstanding.IsEnabled())
            {
                _log.LogInformation("Outstanding reminders skipped: cash collection is switched off for this install.");
                return;
            }

            var masterRepo = scope.ServiceProvider.GetRequiredService<ICashCollectionMasterRepository>();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var collection = scope.ServiceProvider.GetRequiredService<ICashCollectionMasterService>();

            var debtors = (await masterRepo.GetAll())
                .Where(m => m.Outstanding > 0)
                .OrderByDescending(m => m.Outstanding)
                .ToList();

            if (debtors.Count == 0)
            {
                _log.LogInformation("Outstanding reminders: nobody has a balance today.");
                return;
            }

            var usersById = (await userRepo.GetAllUsers()).ToDictionary(u => u.UserMasterID);

            int sent = 0, skipped = 0, failed = 0;
            decimal total = 0m;

            foreach (var debtor in debtors)
            {
                try
                {
                    if (!usersById.TryGetValue(debtor.UserID, out var user) || !user.IsActive)
                    {
                        // A deactivated account still owes the money, but texting it reaches
                        // somebody who can no longer do anything about it. The balance stays on
                        // the report, where an operator can act on it.
                        skipped++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(user.MobileNumber)) { skipped++; continue; }

                    string parentName = user.ParentID.HasValue && usersById.TryGetValue(user.ParentID.Value, out var parent)
                        ? parent.FirmName ?? string.Empty
                        : string.Empty;

                    await collection.SendReminder(user, debtor.Outstanding, parentName);

                    sent++;
                    total += debtor.Outstanding;
                    await Task.Delay(PaceMilliseconds);
                }
                catch (Exception ex)
                {
                    // One bad number must not cost everyone else their reminder.
                    failed++;
                    _log.LogWarning(ex, "Outstanding reminder failed for user {UserId}.", debtor.UserID);
                }
            }

            _log.LogInformation(
                "Outstanding reminders: {Sent} sent, {Skipped} skipped, {Failed} failed. Total outstanding reminded: {Total:F2}.",
                sent, skipped, failed, total);
        }
    }
}
