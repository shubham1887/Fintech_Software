using Project.Processing;

namespace Project.Response.UserwiseLowBalance
{
    public class ViewModel
    {
        private int _UserwiseLowBalanceID;

        public int UserwiseLowBalanceID
        {
            get => _UserwiseLowBalanceID;
            set => _UserwiseLowBalanceID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _WalletName;

        public string WalletName
        {
            get => _WalletName.TrimIfNotNull();
            set => _WalletName = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
    }
}