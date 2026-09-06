namespace FintechApp.Responses.WalletToWalletTransfer
{
    public class WalletTransferResponse
    {
        public string AepsWalletBal { get; set; } = string.Empty;
        public string mainWalletBal { get; set; } = string.Empty;
        public int ReferenceID { get; set; }
        public string Remark { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string DateTime { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
