namespace Project.Message
{
    public class OverdraftMessgae
    {
        public static string RequestFailure = "Overdraft request not submitted";

        public static readonly string FCM_Message_OverDraft_Reject = "Overdraft Rs [Amount] request rejected from [FirmName]";

        public static readonly string InsufficientBalance = "Insufficient balance to accept overdraft request";

        public static string RequestRejectFailure = "Overdraft request not rejected";

        public static readonly string InvalidID = "Please select valid overdraft id";

        public static readonly string InvalidReceiver = "Please select valid Receiver";

        public static string FCM_Message_Overdraft_Transfer = "Overdraft Rs [Amount] received  from [FirmName]";
        public static string OverdraftLimitExceeds = "Currently overdarft limit reached [Amount], Please clear the overdarft amount";

        public static readonly string AcceptSuccess = "Overdraft request accepted successfully";
        public static readonly string RejectSuccess = "Overdraft request rejected successfully";

        public static string OverdraftReverseSucsess = "Overdraft reversed successfully";

        public static string Failure = "Fail: unable to overdraft";
        public static string InsertSuccess = "Overdraft insert successfully";

        public static string Invalid = "Invalid ID";

        public static string InsertDeaultCappingFailure = "Unable to insert";

        public static readonly string UpdateFail = "Overdraft not updated";
        public static readonly string NotFound = "Requested Service not found";

        public static readonly string UpdateSuccess = "Overdraft updated successfully";
    }
}