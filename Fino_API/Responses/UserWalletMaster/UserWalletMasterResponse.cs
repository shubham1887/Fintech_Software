namespace FintechApp.Responses.UserWalletMaster
{
    public class UserWalletMasterResponse
    {
        public int UserWalletMasterID { get; set; }
        public int WalletID { get; set; }
        public decimal Balance { get; set; }
        public int UserID { get; set; }
        public string? BalanceHash { get; set; }
    }
}
