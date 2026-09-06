using Project.Processing;

namespace Project.Response.Overdraft
{
    public class ReverseViewModel
    {
        private int _OverdraftMasterID;

        public int OverdraftMasterID
        {
            get => _OverdraftMasterID;
            set => _OverdraftMasterID = value;
        }

        private string dOC;

        public string DOC
        {
            get => dOC;
            set => dOC = value;
        }

        private decimal amount;

        public decimal Amount
        {
            get => amount;
            set => amount = value;
        }

        private string user;

        public string User
        {
            get => user.TrimIfNotNull();
            set => user = value.TrimIfNotNull();
        }

        private string wallet;

        public string Wallet
        {
            get => wallet.TrimIfNotNull();
            set => wallet = value.TrimIfNotNull();
        }

        public decimal WalletBalance { get; internal set; }
    }
}