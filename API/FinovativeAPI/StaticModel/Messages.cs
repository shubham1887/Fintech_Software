namespace Project.StaticModel
{
    public static class Messages
    {
        public static int ICICI_Static_GUID_LENGETH = 14;// dont change this, because used in ICICI UPI callback, contact to your "Team Lead", if want to change

        public static int DefaultMPin = 1234;
        public static readonly string Percentage = "P";
        public static readonly string PrintNotAvailable = "Print not available for this operator";
        public static readonly string NotSetCallbackUrl = "Callback url not set api user";
        public static readonly string ValidationSucess = "Validation done successfully";

        public static readonly string DOCIntegerWithTwoDigitYear = "yyMMdd";

        public static readonly string DOCDateTimeDisplayFormat = "dd-MMM , hh:mm tt";

        public static readonly string DOCIntegerFomrat = "yyyyMMdd";
        public static readonly string DOCDisplayFormat = "dd-MMM-yyyy hh:mm tt";
        public static readonly string DOCLongDisplayFormat = "dd-MMM-yyyy hh:mm:ss tt";
        public static readonly string DOCLong24HrDisplayFormat = "dd-MM-yyyy HH:mm:ss";

        public static readonly string DateShortFormat = "dd-MMM-yyyy";
        public static readonly string DOCFormat = "dd MMM yyyy";
        public static readonly string DateFormat = "yyyy-MM-dd";
        public static readonly string DateShortIntegerFormat = "ddMMyyyy";

        public static readonly string OnlyDateFormat = "dd-MM-yyyy";

        public static readonly string DocFormat = "yyyy-mm-dd";
        public static readonly string OnlyMonthFormat = "MMM";
        public static readonly string OnlyYearFormat = "yyyy";
        public static readonly string OnlyMonthYearFormat = "MMM-yyyy";
        public static readonly string DOCDayFormat = "dd";

        public static readonly string InvalidUserInput = "Invalid user inputs";
        public static readonly string InvalidAmount = "Invalid amount";
        public static readonly string AmountShouldLessThan5000 = "Invalid amount, Should less than 5000";

        public static readonly string DataNotFound = "Data not found";
        public static readonly string NotActive = "AEPS is not active, please contact to customer care";

        public static readonly string ExceptionOccured = "Something went wrong";
        public static readonly string InboxException = "Exception in inbox, please contact customer care";

        public static readonly string DeductionTransType = "DEDUCTION";

        public static readonly string InValidRequest = "Please send valid request";
        public static readonly string InValidKeyData = "Please send valid key-data";

        public static readonly string InValidProviderOperatorID = "Invalid Provider Operator";

        public static readonly string InvalidPlatform = "Please select valid platform";
        public static readonly string InvalidIMEI = "Invalid IMEI";
        public static readonly string InvalidIP = "Invalid IP";

        public static readonly string InvalidAPIToken = "Invalid API Token";

        public static readonly string InvalidFromDateTodate = "Invalid selected dates";
        public static readonly string Only31DaysDataAllowed = "Only 31 days data allowed";

        public static readonly string InvalidStatus = "Please select valid status";
        public static readonly string State = "Please select State";

        public static readonly string MatcherParamNotFound = "ParamNotMatched";

        public static readonly string InsertLogFail = "Unable insert log";

        internal static readonly string InvalidDate = "Invalid date";
        internal static readonly string NotInProcess = "Transaction not in process";

        public static readonly string InvalidOTPRequest = "Invalid OTP request";
        public static readonly string InvalidOTP = "Please enter a valid OTP";
        public static readonly string InvalidURL = "Invalid URL";

        public static string GoogleFCMSendURL = "https://fcm.googleapis.com/fcm/send";
        public static string ExpoFCMSendURL = "https://exp.host/--/api/v2/push/send";
        internal static string InvalidUser = "Invalid User";
        internal static string InvalidOperatorCode = "Invalid Operator Code";
        internal static string DuplicatePAN = "Duplicate PAN";

        public static readonly string DefaultImagePath = "app-docs/ProfileImage/default.jpg";
        public static readonly string DefaultLogoPath = "app-docs/logo.png";

        public static string InvalidPANNO = "Invalid PAN NO";
        public static string Duplicate = "Duplicate";
        public static string CommssionDecrease = "Service is off contact to distributor";

        public static string RefundedSuccessfully = "Refunded successfully";

        public static string InvalidMoneyTransferID = "Please select valid MoneyTransfer ID";

        public static string InvalidTransactionMasterID = "Please select valid TransactionMaster ID";
        public static string WhiteLableUser = "You don't have permission to access this resource";
        public static string DMTError18 = "ERROR 18 BENEFICIARY BANK DOWN";

        public static string GSTMonthly = "Monthly";

        public static string GSTDaily = "Daily";

        public static readonly string InvalidAuthCode = "Invalid auth code";

        public static string AlreadyRefunded = "Already Refunded";
        public static string NoOTPRequired = "No OTP required";
        public static string OTPSend = "OTP send successfully";
        public static string OnboardingSuccess = "Onboarding successful";
        public static string InsertFailure = "Insert Fail";
        public static string AlreadyExits = "Already exits record";
        public static string InsertSuccess = "Plan added successfully";
        public static string DeleteSuccess = "Plan deleted successfully";

        public static readonly string SettingNotActive = "PG setting is not active, please contact to customer care";
        public static string NotFound = "Record not found";
    }
}