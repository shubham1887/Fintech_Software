namespace FintechApp.Responses.MicroATM
{
    public class InitiateResponse
    {
        public string Message { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CustomerNumber { get; set; } = string.Empty;
        public string UniqueRefNo { get; set; } = string.Empty;
        public string UserMobileNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Slat { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
    }
}
