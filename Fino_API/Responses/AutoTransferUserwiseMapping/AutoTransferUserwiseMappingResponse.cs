namespace FintechApp.Responses.AutoTransferUserwiseMapping
{
    public class AutoTransferUserwiseMappingResponse
    {
        public int AutoTransferUserwiseMappingID { get; set; }
        public int ParentID { get; set; }
        public int UserID { get; set; }
        public bool IsOTPVerified { get; set; }
        public decimal MaxOutstanding { get; set; }
        public decimal RechargeLowBalanceReached { get; set; }
        public decimal RechargeAmountIncreasedBy { get; set; }
        public decimal DMTLowBalanceReached { get; set; }
        public decimal DMTAmountIncreasedBy { get; set; }
        public DateTime DateTime { get; set; }
    }
}
