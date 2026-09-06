namespace FintechApp.Responses.PayoutBeneficiaryOTP
{
    public class PayoutBeneficiaryOTPResponse
    {
    public int PayoutBeneficiaryOTPID { get; set; }
    public string OTP { get; set; } = string.Empty;
    public int PayoutBeneficaryID { get; set; }
    public int UserID { get; set; }
    public bool IsOTPVerified { get; set; }
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
