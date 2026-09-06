namespace Project.StaticModel
{
    public class eKycUrl
    {
        public static readonly string GenerateOtpUrl = "https://pdrs.online/API2/AdharVerification_get_otp?username=[username]&password=[password]&token=[token]&aadhaar_number=[aadhaarNo]&pan=&";
        public static readonly string VerifyOtpUrl = "https://pdrs.online/API2/AdharVerification_submit_otp?username=[username]&password=[password]&token=[token]&client_id=[clientid]&otp=[otp]&mobile_number=[MobileNumber]";
        public static readonly string QuickPanUrl = "https://api.quickekyc.com/api/v1/pan/pan";
        public static readonly string QuickGenerateOtpUrl = "https://api.quickekyc.com/api/v1/aadhaar-v2/generate-otp";
        public static readonly string QuickVerifyOtpUrl = "https://api.quickekyc.com/api/v1/aadhaar-v2/submit-otp";
        public static readonly string QuickUPIUrl = "https://api.quickekyc.com/api/v1/bank-verification/upi-verification";
        public static readonly string IdfyCreateProfileUrl = "https://api.kyc.idfy.com/sync/profiles";
    }
} 
