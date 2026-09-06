namespace FintechApp.Requests.eKYC
{
    public class YesBank_AadharBiometricRequest
    {
        public string RequestID { get; set; } = string.Empty;
        public string FingerPrint { get; set; } = string.Empty;
        public string KYCToken { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string BiometricType { get; set; } = string.Empty;
        public string AadharNo { get; set; } = string.Empty;
        public string WadhValue { get; set; } = string.Empty;
    }
}
