namespace FintechApp.Requests.OutsideRegistration
{
    public class FetchPincodeRequest
    {
        public string Pincode { get; set; } = string.Empty;
    }

    public class RoleForSignUpRequest
    {
        public string ReferralCode { get; set; } = string.Empty;
    }

    public class SignUpGenerateOtpRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
    }

    public class SignUpVerifyOtpRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
    }

    /// <summary>Public "Create account" submission. Mobile must be OTP-verified first.</summary>
    public class OutsideRegisterRequest
    {
        public string PersonName { get; set; } = string.Empty;
        public string FirmName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string WhatsAppNumber { get; set; } = string.Empty;
        public string EmailID { get; set; } = string.Empty;
        public string PANNo { get; set; } = string.Empty;
        public string AadharNo { get; set; } = string.Empty;
        public string GSTNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public int? StateID { get; set; }
        public int UserTypeID { get; set; }
        public string ReferralCode { get; set; } = string.Empty;
    }
}
