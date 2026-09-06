namespace Project.Message
{
    public class GSTMessage
    {
        public static readonly string successupdate = "GST updated successfully";
        public static readonly string failupdate = "Unable to update GST";
        public static readonly string Statussuccessupdate = "GST status updated successfully";
        public static readonly string InvalidRemark = "Invalid remark";
        public static readonly string InvalidStatus = "Invalid status";
        public static readonly string InvalidUser = "Invalid user";
        public static readonly string InvalidGst = "Please select valid user for GST status update";

        public static readonly string NotUpdateStatus = "Status can not update for GST";
        public static readonly string UserGSTSuccessFail = "[SuccessCount] users GST amount successfully and [FailureCount] are unable to update.";
    }
}