namespace FintechApp.Requests.PinGeneration
{
    public class PinGenerationRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Pin { get; set; }
        public int ConfirmPin { get; set; }
        public string DeviceID { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
    }

    public class PinVerifyRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
        public int Pin { get; set; }
    }

    public class ForgotMpinRequest
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string PANNo { get; set; } = string.Empty;
    }
}
