namespace Project.Message
{
    public class eKYCMessage
    {
        public static readonly string NotActive = "UPI Verify not active, please contact to customer care";
        public static readonly string InvalidOTP = "Invalid OTP";
        public static readonly string InvalidID = "Invalid ID";
        public static readonly string InvalidClientID = "Invalid client ID";

        public static readonly string InvalidMobile = "Invalid Mobile";
        public static readonly string InvalidPAN = "Invalid PAN";

        public static readonly string InvalidAadhar = "Invalid Aadhar";
        public static readonly string InvalidEMail = "Invalid Email";
        public static readonly string InvalidGSTNumber = "Invalid GST number";
        public static readonly string DuplicatePAN = "This pan number is already in use by another user.";
        public static readonly string DuplicateGST = "This GST number is already in use by another user.";
        public static readonly string DuplicateAadhar = "This aadhar number is already in use by another user.";
        public static readonly string DuplicatVerifyPAN = "Your pan verification is already done ";
        public static readonly string DuplicatVerifyGST = "Your gst verification is already done";
        public static readonly string NotFound = "Token and URL not available";
        public static readonly string URLNotFound = "Token and URL not available";
        public static readonly string SendOTP = "Otp send successfully";
        public static readonly string VerifyedOTP = "Otp Verified successfully";
        public static readonly string EkycFailed = "Ekyc added failed , please try after sometimes.";
        public static readonly string EkycSuccessful = "Ekyc Register Successful";
        public static readonly string OtpTokenNotFound = "OtpToken not found, please try after sometime";
        public static readonly string KYCTokenNotFound = "KycToken not found, please try after sometime";

    }
}