namespace Project.Message
{
    public class AutoTransferMessages
    {
        public static readonly string InValidUser = "Invalid User";
        public static readonly string InValidOTP = "Invalid OTP";
        public static readonly string InValidDMTAmountIncreasedBy = "Invalid DMTAmountIncreasedBy";
        public static readonly string InValidDMTLowBalanceReached = "Invalid DMTLowBalanceReached";
        public static readonly string InValidMaxOutstanding = "Invalid MaxOutstanding";
        public static readonly string InValidRechargeLowBalanceReached = "Invalid RechargeLowBalanceReached";
        public static readonly string InValidRechargeAmountIncreasedBy = "Invalid RechargeAmountIncreasedBy";

        public static readonly string OTPVerifySuccess = "OTP verified successfully";
        public static readonly string OTPSuccess = "OTP sent successfully on your registered mobile number.";

        public static readonly string NotFound = "Data not found";
        public static readonly string InsertFail = "Failed to insert userwise setting for auto balance transfer.";
        public static readonly string AlreadyExist = "Auto balancee transfer setting already exist.";
        public static readonly string InsertSuccess = "Auto balancee transfer setting  added successfully.";
        public static readonly string UpdateFail = "Auto balancee transfer setting  not updated.";
        public static readonly string UpdateSuccess = "Auto balancee transfer setting updated successfully.";

        public static string DeleteFail = "Auto balancee transfer setting not deleted";
        public static string DeleteSuccess = "Auto balancee transfer setting deleted successfully";
    }
}