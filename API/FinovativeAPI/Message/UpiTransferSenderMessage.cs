namespace FinovativeAPI.Message
{
    public class UpiTransferSenderMessage
    {
        public static readonly string InvalidName = "Please enter valid sender name, length must be at least 4 characters";
        public static readonly string InvalidGUID = "Please enter valid guid";
        public static readonly string InvalidCity = "Please enter valid city";
        public static readonly string InvalidState = "Please enter valid state";
        public static readonly string InvalidCountry = "Please enter valid country";
        public static readonly string InvalidAddress1 = "Please enter valid address1";
        public static readonly string InvalidAddress2 = "Please enter valid address2";

        public static readonly string InvalidOTP = "Please enter valid OTP";
        public static readonly string InvalidMobile = "Please enter valid sender mobile";
        public static readonly string VerifyOTP = "Please verify sender OTP";
        public static readonly string NotRegistered = "Sender not registered";
        public static readonly string NotVerified = "Sender not verified";
        public static readonly string NotFound = "Sender not found";

        public static readonly string OTPSentOnMobileNumer = "OTP sent on mobile number";

        public static readonly string InvalidSender = "Please select valid sender";
        public static readonly string InvalidBeneficiary = "Please select valid beneficiary";
        public static readonly string InvalidUserID = "Please select valid User ID";

        public static readonly string UpdateSuccess = "Sender Verified successfully";
        public static readonly string AlreadyVerified = "Sender already verified";
        public static readonly string MobileAlreadyExits = "Sender mobile already exits";
        public static readonly string InsertFailure = "Sender not added";
        public static readonly string InsertSuccess = "Sender added successfully";

        public static readonly string OTP = "[OTP] is otp for your registration";
       
    }
}
