namespace Project.Message
{
    public class FundMessage
    {
        public static readonly string InValidRemark = "Please enter valid remark";

        public static readonly string InvalidReceiver = "Please select valid Receiver";

        public static readonly string TransactionRequestSuccess = "Fund transfer request submitted successfully";
        public static readonly string ReverseTransactionRequestSuccess = "Fund reverse request submitted successfully";

        public static readonly string RequestFailure = "Fund transfer request not submitted";

        public static readonly string InsufficientBalance = "Insufficient balance to accept fund transaction request";

        public static readonly string AlreadProcessd = "Fund transaction already processed.";

        public static readonly string InvalidID = "Please select valid fund transaction id";

        public static readonly string AcceptSuccess = "Fund transfer request accepted successfully";
        public static readonly string RejectSuccess = "Fund transfer request rejected successfully";

        public static string RequestAcceptFailure = "Fund transfer request not accepted";

        public static string RequestRejectSuccess = "Fund transfer request rejected successfully";

        public static string RequestRejectFailure = "Fund transfer request not rejected";

        public static string SameUser = "User can not transfer balance to self account";

        public static string LowBalance = "Insufficient e-wallet balance";

        public static readonly string Outstandingfalling = "Currently outstanding reached [Amount], Please clear the outstanding amount";

        public static string FundTransactionSucsess = "Fund transfered successfully";
        public static string FundReverseSucsess = "Fund reversed successfully";

        public static string InvalidUser = "Invalid user for fund transfer";

        public static string Failure = "Fail: unable to fund transfer";

        public static string Duplicate = "Duplicate Transaction: please try after 2 minutes";

        public static readonly string InvalidDate = "Invalid date";

        public static readonly string Invalidfrom = "Invalid from";

        public static readonly string InvalidStatus = "Invalid status";

        public static readonly string InvalidType = "Invalid type";

        public static readonly string OutstandingExceeds = "Outstanding Exceeds";

        public static readonly string InvalidRemark = "Invalid Remark";

        public static readonly string FCM_Message_Fund_Transfer = "Fund Rs [Amount] received  from [FirmName]";
        public static readonly string FCM_Message_PG_Fund_Received = "Fund Rs [Amount] received  from [FirmName] through Payment Gateway";

        public static readonly string FCM_Message_Fund_Reject = "Fund Rs [Amount] request rejected from [FirmName]";

        public static readonly string FCM_Message_Fund_Request = "Fund Request of Rs [Amount] from [FirmName]";

        public static readonly string FCM_Message_Fund_Reverse = "Fund reversed of Rs [Amount] from [FirmName]";
        public static readonly string InvalidAccess = "You do not have access.";

        public static readonly string InvalidSender = "Please select valid sender";

        public static readonly string RefreshBalanceFlag = "Refresh Balance";

        public static readonly string RefreshOperatorFlag = "Refresh Operator";

        public static readonly int FundTransferDuplicateTimeSpan = 15;

        public static string DefaultDuplicateFundReamrks = "No";

        public static string DuplicateUTR = "Duplicate UTR Number";

        public static readonly string InvalidBank = "Please select bank name";
        
    }
}