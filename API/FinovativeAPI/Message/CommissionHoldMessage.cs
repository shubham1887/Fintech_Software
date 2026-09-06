namespace Project.Message
{
    public class CommissionHoldMessage
    {
        public static readonly string InsertSuccess = "Request send successfully";
        public static readonly string InsertFail = " Request send fail";
        public static readonly string Statussuccessupdate = "Request status updated successfully";
        public static readonly string InvalidRemark = "Invalid remark";
        public static readonly string InvalidStatus = "Invalid status";
        public static readonly string InvalidMonth = "Invalid month";
        public static readonly string InvalidYear = "Invalid year";
        public static readonly string InvalidUser = "Invalid user";
        public static readonly string InvalidWallet = "Invalid wallet";
        public static readonly string InvalidID = "Invalid id";
        public static readonly string NotUpdateStatus = "Status can not update for request";
        public static string AlreadyExists = "Request already rasied";

        public static readonly string InvalidRequest = "Please select valid request";

        public static readonly string NotUpdateData = "Commission updated but can not update from transaction";

        public static readonly string GenerateSuccess = "Request generated successfully";

        public static readonly string AbortedRequest = "Commission hold setting is off for system";
        public static readonly string NotFound = "Data not found";
        public static readonly string NotHold = "Commission not holded for any user";

        public static readonly string AllTransferFail = "Commission is not transfered";
        public static readonly string UserCommTransferSuccessFail = "[SuccessCount] user's commission transfered successfully and [FailureCount] are unable to transfer.";
    }
}