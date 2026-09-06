namespace FintechApp.Responses.AEPS
{
    public class AEPSSettingResponse
    {
        public int AEPSSettingID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MerchantID { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Slat { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsActiveWeb { get; set; }
        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string Mode { get; set; } = string.Empty;
        public bool IsMerchantVerify { get; set; }
    }

    public class AEPSTransactionStatusResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string APIUserRequestID { get; set; } = string.Empty;
    }

    public class PIDOptionResponse
    {
        public string Message { get; set; } = string.Empty;
        public string PID { get; set; } = string.Empty;
    }
}
