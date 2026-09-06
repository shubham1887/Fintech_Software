using Project.Processing;

namespace Project.Response.Wallet
{
    public class DetailViewModel
    {
        public int WalletID { get; set; }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private float _Balance;

        public float Balance
        {
            get => _Balance;
            set => _Balance = value;
        }

        private string _ShortName;

        public string ShortName
        {
            get => _ShortName.TrimIfNotNull();
            set => _ShortName = value.TrimIfNotNull();
        }

        private string _BalanceHash;

        internal string BalanceHash
        {
            get => _BalanceHash.TrimIfNotNull();
            set => _BalanceHash = value.TrimIfNotNull();
        }
    }
}