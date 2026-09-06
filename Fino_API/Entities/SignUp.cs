namespace FintechApp.Entities
{
    public class SignUp
    {
        public int SignUpID { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string PANNo { get; set; } = string.Empty;
        public string ReferralCode { get; set; } = string.Empty;
        public string EmployeeID { get; set; } = string.Empty;
        public string OrderID { get; set; } = string.Empty;
        public string ProfileID { get; set; } = string.Empty;
        public string RedirectURL { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
    }
}
