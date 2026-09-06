namespace FintechApp.Requests.HitachiYesBankAEPS
{
    public class YesBank_MinistatementRequest
    {
        public string AadhaarNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string ConsumerNumber { get; set; } = string.Empty;
        public string IIN { get; set; } = string.Empty;
        public string FingerPrint { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public string APIUserRequestID { get; set; } = string.Empty;
        public string authType { get; set; } = string.Empty;
    }
}
