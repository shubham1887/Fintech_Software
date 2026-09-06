namespace Project.Response.User
{
    public class WalletView
    {
        private string _WalletName;

        public string WalletName
        {
            get => _WalletName;
            set => _WalletName = value;
        }

        private float _WalletBalance;

        public float WalletBalance
        {
            get => _WalletBalance;
            set => _WalletBalance = value;
        }

        private float _TotalBalance;

        public float TotalBalance
        {
            get => _TotalBalance;
            set => _TotalBalance = value;
        }
    }
}