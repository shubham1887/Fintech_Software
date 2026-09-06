using Project.DAL;
using Project.Response.Appbar;
using Project.StaticModel;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class AppbarBal
    {
        internal async Task<ViewModel> Refresh(int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (UserTypeID == (int)UserType.Support)
                UserID = Company.AdminUserID;

            var PendingKYCTask = new UserDocumentMasterDal().SelectPendingKYCCount();
            var FundRequestTask = new FundTransactionDal().SelectFundRequestCount(UserID);
            var FundReverseRequestTask = new FundTransactionDal().SelectFundReverseRequestCount(UserID);
            var ComplaintTask = new ComplaintMasterDal().SelectComplaintCount();
            var PendingTransactionTask = new TransactionDAL().SelectPendingTransactionCount();
            var PendingDMTTransactionTask = new TransactionDAL().SelectPendingDMTTransactionCount();
            var NotificationTask = new NewsDAL().SelectCountByUserID(UserID);
            var PendingTicketTask = new TicketMasterDAL().SelectPendingTicketCount();
            var CallMeRequestTask = new CallMeRequestDal().SelectPendingCallMeRequestCount(UserID);
            var PasswordExpiraryTask = new PasswordChangeHistoryBal().SelectPasswordExpiryDays(UserID);

            var PendingUPITransactionTask = new TransactionDAL().PendingUPITransactionCount();

            await Task.WhenAll(PendingKYCTask, FundReverseRequestTask, FundRequestTask, ComplaintTask, PendingTransactionTask, NotificationTask, PendingTicketTask, PasswordExpiraryTask);

            ViewModel response = new ViewModel
            {
                PendingKYCCount = PendingKYCTask?.Result ?? 0,
                FundRequesCount = FundRequestTask?.Result ?? 0,
                FundReverseRequestCount = FundReverseRequestTask?.Result ?? 0,
                ComplaintCount = ComplaintTask?.Result ?? 0,
                PendingTransactionCount = PendingTransactionTask?.Result ?? 0,
                PendingDMTTransactionCount = PendingDMTTransactionTask?.Result ?? 0,
                NotificationCount = NotificationTask?.Result ?? 0,
                CallMeRequestCount = CallMeRequestTask?.Result ?? 0,
                PendingTicketCount = PendingTicketTask?.Result ?? 0,
                PasswordExpiraryDays = PasswordExpiraryTask?.Result ?? 0,
                SubscriptionDays = 0,
                PendingUPITransactionCount = PendingUPITransactionTask?.Result ?? 0,
            };
            return response;
        }

        public async Task<ViewModel> RefreshSD(int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (UserTypeID == (int)UserType.Support)
                UserID = Company.AdminUserID;

            var FundRequestTask = new FundTransactionDal().SelectFundRequestCount(UserID);

            await Task.WhenAll(FundRequestTask);

            ViewModel response = new ViewModel
            {
                FundRequesCount = FundRequestTask?.Result ?? 0,
            };

            return response;
        }
    }
}