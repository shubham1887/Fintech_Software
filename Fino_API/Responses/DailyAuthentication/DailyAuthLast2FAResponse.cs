namespace FintechApp.Responses.DailyAuthentication
{
    public class DailyAuthLast2FAResponse
    {
        public int AuthenticationID { get; set; }
        public int UserID { get; set; }
        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public string Pipe { get; set; } = string.Empty;
        public DateTime? DOCDateTime { get; set; }
        public bool Is2FA { get; set; }
        public bool IsMerchantVerify { get; set; }
    }
}
