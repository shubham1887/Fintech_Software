namespace FintechApp.Responses.PendingRegistration
{
    public class PendingRegistrationResponse
    {
        public int PendingRegistrationID { get; set; }
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
        public string StateName { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public string UserTypeName { get; set; } = string.Empty;
        public string ReferralCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RejectReason { get; set; } = string.Empty;
        public bool IsMobileVerified { get; set; }
        public int? CreatedUserID { get; set; }
        public string DateTime { get; set; } = string.Empty;
    }
}
