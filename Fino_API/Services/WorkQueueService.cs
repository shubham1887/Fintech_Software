using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests;
using FintechApp.Responses.WorkQueue;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// The counts behind the strip that sits on every screen: pending recharges, fund requests
    /// waiting on me, complaints, tickets, unread alerts.
    ///
    /// Every one of these already had a screen - and an operator who only found out there was
    /// work waiting by opening that screen and looking. The point of this service is that the
    /// answer arrives without being asked for.
    ///
    /// Two rules hold for every queue below:
    ///
    ///  1. Role decides which queues exist. A retailer has no "pending recharge" desk to work,
    ///     so they are not given a chip that would only ever read zero. The list the caller gets
    ///     back IS the list they may see - the client does no filtering of its own.
    ///
    ///  2. The count is scoped to the caller. Fund requests count the ones addressed to *me*;
    ///     complaints and tickets count mine unless I am the one who works the queue. A count a
    ///     user cannot act on is worse than no count.
    ///
    /// Adding a queue is one entry in <see cref="GetCounts"/> plus one icon on the client.
    /// </summary>
    public class WorkQueueService : IWorkQueueService
    {
        private readonly ITransactionMasterRepository _txnRepo;
        private readonly IFundTransactionRepository _fundRepo;
        private readonly IComplaintMasterRepository _complaintRepo;
        private readonly ITicketMasterRepository _ticketRepo;
        private readonly IAppNotificationService _notifications;
        private readonly Logger _logger = new();

        public WorkQueueService(
            ITransactionMasterRepository txnRepo,
            IFundTransactionRepository fundRepo,
            IComplaintMasterRepository complaintRepo,
            ITicketMasterRepository ticketRepo,
            IAppNotificationService notifications)
        {
            _txnRepo = txnRepo;
            _fundRepo = fundRepo;
            _complaintRepo = complaintRepo;
            _ticketRepo = ticketRepo;
            _notifications = notifications;
        }

        /// <summary>Admin and support work the platform's queues; everyone else works their own.</summary>
        private static bool RunsTheQueue(int userTypeId) =>
            userTypeId == UserTypeCode.Admin || userTypeId == UserTypeCode.SupportUser;

        /// <summary>
        /// Every chip is a link, so a chip may only be sent to a role whose menu actually contains
        /// the screen behind it (MenuService is the source of truth for that). A count that opens
        /// a permission error is worse than no count at all — these two lists mirror the "Fund"
        /// group's own role gates and must be changed together with it.
        /// </summary>
        private static readonly int[] FundQueueRoles =
            { UserTypeCode.Admin, UserTypeCode.SuperDistributor, UserTypeCode.Distributor,
              UserTypeCode.SDFOS, UserTypeCode.DFOS };

        private static readonly int[] FundRaiserRoles = { UserTypeCode.Retailer };

        private static bool CanOpen(int userTypeId, int[] roles) => Array.IndexOf(roles, userTypeId) >= 0;

        /// <summary>A ticket nobody has finished with. Matches the Tickets screen's own reading.</summary>
        private static bool TicketIsOpen(string? status)
        {
            string s = (status ?? string.Empty).Trim();
            return !s.EqualsIgnoreCase("Resolved") && !s.EqualsIgnoreCase("Closed");
        }

        public async Task<WorkQueueResponse> GetCounts(KeyDataModel caller)
        {
            var queues = new List<WorkQueueCount>();
            bool runsQueue = RunsTheQueue(caller.UserTypeID);

            // ---------------------------------------------------------- transactions
            // Only the people who can actually settle a stuck transaction are shown it. A retailer
            // seeing "37 pending" they cannot touch is noise, and the number would be the whole
            // platform's anyway.
            if (runsQueue)
            {
                queues.Add(new WorkQueueCount
                {
                    Key = "pending-recharge",
                    Label = "Pending recharges",
                    Count = await Safe(() => _txnRepo.GetPendingCount()),
                    Url = "/Pending",
                    Tone = "danger",
                });

                queues.Add(new WorkQueueCount
                {
                    Key = "pending-dmt",
                    Label = "Pending transfers",
                    Count = await Safe(() => _txnRepo.GetPendingDMTCount()),
                    Url = "/PendingDmt",
                    Tone = "danger",
                });
            }

            // ----------------------------------------------------------------- fund
            // Two different questions, and which one a role cares about depends on which side of
            // the request they are on:
            //
            //   an approver asks  "what is waiting for MY decision?"   (RequestToID = me)
            //   a raiser asks     "has MY request been decided yet?"   (ReceiverID  = me)
            //
            // Showing an approver's count to a retailer would be a permanent zero; showing the
            // raiser's count to an approver would bury the one number they can act on.
            if (CanOpen(caller.UserTypeID, FundQueueRoles))
            {
                queues.Add(new WorkQueueCount
                {
                    Key = "fund-request",
                    Label = "Fund requests waiting",
                    Count = await Safe(() => _fundRepo.GetPendingRequestCount(caller.UserID)),
                    Url = "/Fund",
                    Tone = "warning",
                });
            }
            else if (CanOpen(caller.UserTypeID, FundRaiserRoles))
            {
                queues.Add(new WorkQueueCount
                {
                    Key = "my-fund-request",
                    Label = "My fund requests pending",
                    Count = await MyPendingFundRequests(caller.UserID),
                    // The same queue screen: Fund/SelectRequestByFilter returns requests raised TO
                    // the caller AND requests they raised, so a retailer lands on their own list.
                    Url = "/Fund",
                    Tone = "warning",
                });
            }

            // The reverse-request REPORT is an admin screen and nobody else's menu carries it.
            if (caller.UserTypeID == UserTypeCode.Admin)
            {
                queues.Add(new WorkQueueCount
                {
                    Key = "fund-reverse-request",
                    Label = "Reverse requests waiting",
                    Count = await Safe(() => _fundRepo.GetPendingReverseRequestCount(caller.UserID)),
                    Url = "/FundReverseRequestReport",
                    Tone = "warning",
                });
            }

            // ------------------------------------------------------------ complaints
            queues.Add(new WorkQueueCount
            {
                Key = "complaint",
                Label = runsQueue ? "Open complaints" : "My open complaints",
                Count = await ComplaintCount(caller, runsQueue),
                Url = "/Complaints",
                Tone = "warning",
            });

            // --------------------------------------------------------------- tickets
            queues.Add(new WorkQueueCount
            {
                Key = "ticket",
                Label = runsQueue ? "Open tickets" : "My open tickets",
                Count = await TicketCount(caller, runsQueue),
                Url = "/TicketReport",
                Tone = "info",
            });

            // ---------------------------------------------------------------- alerts
            // The bell already carries this number, but the strip is the one place an operator
            // looks for "is anything waiting", so it belongs in the same row.
            queues.Add(new WorkQueueCount
            {
                Key = "unread-alert",
                Label = "Unread notifications",
                Count = await Safe(() => _notifications.GetUnreadCount(caller.UserID)),
                Url = "/Notifications",
                Tone = "info",
            });

            return new WorkQueueResponse
            {
                Queues = queues,
                Total = queues.Sum(q => q.Count),
            };
        }

        /// <summary>
        /// Fund-load requests this user raised that nobody has decided yet. The raiser is the
        /// RECEIVER of the money (see FundService.RequestInsert), which is why this is not the
        /// same query as the approver's count.
        /// </summary>
        private Task<int> MyPendingFundRequests(int userId) =>
            Safe(async () =>
            {
                var all = await _fundRepo.GetAllFundTransactions();
                return all.Count(f => f.ReceiverID == userId
                    && f.Status == FundStatusProcess
                    && (f.Type == FundTypeTransferRequest || f.Type == FundTypeReverseRequest));
            });

        // Spellings owned by FundService; repeated here rather than made public, so this read-only
        // counter cannot become a reason to change how the fund module writes them.
        private const string FundStatusProcess = "PROCESS";
        private const string FundTypeTransferRequest = "TRANSFER_REQUEST";
        private const string FundTypeReverseRequest = "REVERSE_REQUEST";

        private Task<int> ComplaintCount(KeyDataModel caller, bool runsQueue) =>
            Safe(async () =>
            {
                var rows = runsQueue
                    ? await _complaintRepo.GetAll()
                    : await _complaintRepo.GetByUserID(caller.UserID);
                return rows.Count(c => !c.IsDeleted && ComplaintStatus.IsOpen(c.ComplaintStatus));
            });

        private Task<int> TicketCount(KeyDataModel caller, bool runsQueue) =>
            Safe(async () =>
            {
                var all = await _ticketRepo.GetAllTicketMasters();
                return all.Count(t => !t.IsDelete && !t.IsDeleted
                    && TicketIsOpen(t.Status)
                    && (runsQueue || t.UserID == caller.UserID));
            });

        /// <summary>
        /// A queue that cannot be counted contributes zero, not an exception. This strip renders on
        /// EVERY screen - one broken repository must never be able to blank the whole console.
        /// </summary>
        private async Task<int> Safe(Func<Task<int>> read)
        {
            try { return await read(); }
            catch (Exception ex) { _logger.Write(ex); return 0; }
        }
    }
}
