using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Responses.Appbar;

namespace FintechApp.Services
{
    public class AppbarService : IAppbarService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUserDocumentMasterRepository _docRepo;
        private readonly IFundTransactionRepository _fundRepo;
        private readonly IComplaintMasterRepository _complaintRepo;
        private readonly ITransactionMasterRepository _transactionRepo;
        private readonly INewsMasterRepository _newsRepo;
        private readonly ICallMeRequestRepository _callMeRepo;
        private readonly ITicketMasterRepository _ticketRepo;
        private readonly ISettingMasterRepository _settingRepo;
        private readonly IPasswordChangeHistoryRepository _passwordHistoryRepo;
        private readonly Logger _logger;

        private const int SupportUserTypeId = 6;
        private const int AdminUserTypeId = 1;
        private const int DefaultPasswordExpireDays = 30;

        public AppbarService(
            IUserRepository userRepo,
            IUserDocumentMasterRepository docRepo,
            IFundTransactionRepository fundRepo,
            IComplaintMasterRepository complaintRepo,
            ITransactionMasterRepository transactionRepo,
            INewsMasterRepository newsRepo,
            ICallMeRequestRepository callMeRepo,
            ITicketMasterRepository ticketRepo,
            ISettingMasterRepository settingRepo,
            IPasswordChangeHistoryRepository passwordHistoryRepo)
        {
            _userRepo = userRepo;
            _docRepo = docRepo;
            _fundRepo = fundRepo;
            _complaintRepo = complaintRepo;
            _transactionRepo = transactionRepo;
            _newsRepo = newsRepo;
            _callMeRepo = callMeRepo;
            _ticketRepo = ticketRepo;
            _settingRepo = settingRepo;
            _passwordHistoryRepo = passwordHistoryRepo;
            _logger = new Logger();
        }

        public async Task<AppbarResponse> Refresh(int userId)
        {
            userId = await ResolveUserId(userId);

            // Shared scoped DbContext — await sequentially (concurrent EF queries on one
            // context throw "A second operation was started on this context instance").
            var pendingKyc = await CountPendingKyc();
            var fundRequest = await _fundRepo.GetPendingRequestCount(userId);
            var fundReverse = await _fundRepo.GetPendingReverseRequestCount(userId);
            var complaint = await CountPendingComplaints();
            var pendingTxn = await _transactionRepo.GetPendingCount();
            var pendingDmt = await _transactionRepo.GetPendingDMTCount();
            var pendingUpi = await _transactionRepo.GetPendingUPICount();
            var notif = await CountUnviewedNews(userId);
            var callMe = await CountPendingCallMe(userId);
            var ticket = await CountPendingTickets();
            var pwdDays = await GetPasswordExpiryDays(userId);

            return new AppbarResponse
            {
                PendingKYCCount = pendingKyc,
                FundRequesCount = fundRequest,
                FundReverseRequestCount = fundReverse,
                ComplaintCount = complaint,
                PendingTransactionCount = pendingTxn,
                PendingDMTTransactionCount = pendingDmt,
                PendingUPITransactionCount = pendingUpi,
                NotificationCount = notif,
                CallMeRequestCount = callMe,
                PendingTicketCount = ticket,
                PasswordExpiraryDays = pwdDays,
                SubscriptionDays = 0
            };
        }

        public async Task<AppbarResponse> RefreshSD(int userId)
        {
            userId = await ResolveUserId(userId);
            int fundRequestCount = await _fundRepo.GetPendingRequestCount(userId);
            return new AppbarResponse { FundRequesCount = fundRequestCount };
        }

        private async Task<int> ResolveUserId(int userId)
        {
            try
            {
                var user = await _userRepo.GetUserById(userId);
                if (user?.UserTypeID == SupportUserTypeId)
                {
                    var admins = await _userRepo.GetUsersByUserType(AdminUserTypeId);
                    return admins.FirstOrDefault()?.UserMasterID ?? userId;
                }
            }
            catch (Exception ex) { _logger.Write(ex); }
            return userId;
        }

        private async Task<int> CountPendingKyc()
        {
            var docs = await _docRepo.GetAll();
            return docs.Count(d => d.Status != null && d.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) && !d.IsDeleted);
        }

        private async Task<int> CountPendingComplaints()
        {
            var complaints = await _complaintRepo.GetAll();
            return complaints.Count(c => c.ComplaintStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase) || c.ComplaintStatus.Equals("Open", StringComparison.OrdinalIgnoreCase));
        }

        private async Task<int> CountUnviewedNews(int userId)
        {
            var news = await _newsRepo.GetAll();
            return news.Count(n => n.UserID == userId && !n.IsView && !n.IsDeleted);
        }

        private async Task<int> CountPendingCallMe(int userId)
        {
            var requests = await _callMeRepo.GetAllCallMeRequests();
            return requests.Count(r => r.UplineUserID == userId && r.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) && !r.IsDeleted);
        }

        private async Task<int> CountPendingTickets()
        {
            var tickets = await _ticketRepo.GetAllTicketMasters();
            return tickets.Count(t => t.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) && !t.IsDelete);
        }

        private async Task<int> GetPasswordExpiryDays(int userId)
        {
            try
            {
                int maxDays = DefaultPasswordExpireDays;
                var setting = await _settingRepo.GetByKey("PasswordExpireDays");
                if (setting != null && int.TryParse(setting.Value, out int parsed))
                    maxDays = parsed;

                DateTime lastChanged = DateTime.UtcNow;
                var history = await _passwordHistoryRepo.GetByUserID(userId);
                var latest = history.OrderByDescending(h => h.DateTime).FirstOrDefault();

                if (latest != null)
                    lastChanged = latest.DateTime;
                else
                {
                    var user = await _userRepo.GetUserById(userId);
                    if (user != null) lastChanged = user.DateTime;
                }

                int daysSinceChange = (int)(DateTime.UtcNow - lastChanged).TotalDays;
                return Math.Abs(daysSinceChange - maxDays);
            }
            catch (Exception ex) { _logger.Write(ex); return 0; }
        }
    }
}
