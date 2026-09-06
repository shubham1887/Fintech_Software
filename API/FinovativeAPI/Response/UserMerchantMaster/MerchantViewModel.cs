namespace Project.Response.UserMerchantMaster
{
    public class MerchantViewModel
    {
        public int? UserID { get; set; }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }
    }
}