namespace FintechApp.Requests.DMTSender
{
    public class AddSenderRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
        public int ProviderID { get; set; }
    }

    public class VerifySenderOTPRequest
    {
        public string Mobile { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
    }

    public class ResendSenderOTPRequest
    {
        public string Mobile { get; set; } = string.Empty;
    }

    public class SelectSenderRequest
    {
        public string Mobile { get; set; } = string.Empty;
        public int ProviderID { get; set; }
    }

    public class SelectSenderByAccountRequest
    {
        public string AccountNumber { get; set; } = string.Empty;
    }
}
