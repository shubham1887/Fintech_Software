namespace Project.Message
{
    public class ComplaintMessage
    {
        public static readonly string InvalidCustomerRemark = "Please enter valid remark";

        public static readonly string InvalidComplaint = "Please select valid complaint";
        public static readonly string TransctionAlreadyFailed = "Transaction is already failed";

        public static string InsertFailure = "Complaint not accepted";
        public static string InsertSuccess = "Complaint added successfully";
        public static string AlreadyExists = "Complaint already rasied";

        public static string UpdateFailure = "Complaint not updated";
        public static string UpdateSuccess = "Complaint updated successfully";
        public static string RejectSuccess = "Complaints rejected successfully";
        public static string AcceptSuccess = "Complaints accepted successfully";

        public static string ComplaintAddMessageForAdmin = "Complaint for [Number] with [Amount] by [FirmName]";

        public static string FCM_Message_Complaint_Add = "Complaint of recharge [ConsumerNumber]  from [FirmName]";
        public static string FCM_Message_Complaint_Accept = "Complaint accepted [ConsumerNumber] with [Amount] by [FirmName]";
        public static string FCM_Message_Complaint_Reject = "Complaint rejected [ConsumerNumber] with [Amount] by [FirmName]";
    }
}