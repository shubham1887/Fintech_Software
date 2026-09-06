namespace Project.Message
{
    public class AccountOpenSettingMessage
    {
        internal static string InvalidName = "Please enter valid name";
        internal static string InValidOperatorID = "Please enter valid operator";

        internal static string InvalidUserName = "Please enter valid user name";
        internal static string InvalidMerchantID = "Invalid MerchantID";

        public static string InvalidToken = "Please enter valid Token";
        public static string InvalidCallBackURL = "Please enter valid callback url";

        public static string InvalidSetting = "Please select valid settings";

        public static string InvalidSplitAmount = "Please enter minimum 5000 split amount";
        public static string InvalidSenderLimit = "Please enter minimum 25000 sender limit";
        public static string InvalidMinimumAmount = "Please enter minimum 10 amount";

        public static string NotFound = "Settings not found";

        public static string InvalidActive = "Invalid Active provider";

        public static string InsertSuccess = "Settings added successfully";
        public static string InsertFail = "Settings not added";

        public static string UpdateSuccess = "Settings updated successfully";
        public static string UpdateFail = "Settings not updated successfully";
        public static string DeactiveUPITrasfer = "Account Open setting is not active, please contact to customer care";

        public static readonly string NotActive = "Account Opening is not active, please contact customer care";
    }
}