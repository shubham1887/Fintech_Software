using Project.Processing;

namespace Project.Response.UserBalanceCapping
{
    public class ViewModel
    {
        private int _UserBalanceCappingID;

        public int UserBalanceCappingID
        {
            get => _UserBalanceCappingID;
            set => _UserBalanceCappingID = value;
        }

        private int userID;

        public int UserID
        {
            get => userID;
            set => userID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private string userName;

        public string UserName
        {
            get => userName.TrimIfNotNull();
            set => userName = value.TrimIfNotNull();
        }

        private string _Wallet;

        public string Wallet
        {
            get => _Wallet.TrimIfNotNull();
            set => _Wallet = value.TrimIfNotNull();
        }
    }
}