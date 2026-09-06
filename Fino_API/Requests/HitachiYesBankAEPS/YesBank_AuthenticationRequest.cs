namespace FintechApp.Requests.HitachiYesBankAEPS
{
    public class YesBank_AuthenticationRequest
    {
        public string AadhaarNumber { get; set; } = string.Empty;
        public string ConsumerNumber { get; set; } = string.Empty;
        public string FingerPrint { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public string APIUserRequestID { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
    }
}
