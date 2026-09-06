namespace Project.Message
{
    public class CashCollectionMessage
    {
        public static readonly string NotAllowedCashCollection = "Cash collection not allowed, please contact customer care";
        public static readonly string InsertSuccess = "Cash collected successfully";
        public static readonly string Insertfailure = "Cash not collected";
        public static readonly string SendMessage = "Outstanding message send successfully";

        public static string FCM_Message_Cash_Collection = "Rs [Amount] collected by [FirmName] and new Outstanding is [Outstanding]";
        public static string FCM_Message_Cash_Collection_Reverse = "Rs [Amount] reversed  and new Outstanding is [Outstanding]";
        public static readonly string ReverseSuccess = "Cash collection reversed successfully";
        public static readonly string ReverseFailure = "Cash collection not reversed";
        public static readonly string InValidOutstanding = "Invalid Cash collection Amount";
    }
}