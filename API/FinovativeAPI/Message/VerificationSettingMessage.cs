namespace Project.Message
{
    public class VerificationSettingMessage
    {
        internal static string InvalidName = "Please enter valid name";

        internal static string InvalidUserName = "Please enter valid user name";

        public static string InvalidToken = "Please enter valid Token";
        public static string InvalidCallBackURL = "Please enter valid callback url";

        public static string InvalidSetting = "Please select valid settings";
        public static string NotFound = "Settings not found";

        public static string InvalidActiveVerification = "Invalid verification";

        public static string SettingNotAcive = "Verification setting not acive, please use another verification setting from dashboard menu";

        public static string InsertSuccess = "Settings added successfully";
        public static string InsertFail = "Settings not added";

        public static string UpdateSuccess = "Settings updated successfully";
        public static string UpdateFail = "Settings not updated successfully";
        public static readonly string DataNotFound = "Data not found";
        public static string MissingProviderID = "Providerid is 0. unable to proceed";

    }
}