namespace Project.Response.Wallet
{
    public class ViewModel
    {
        private int _Balance;

        public int Balance
        {
            get => _Balance;
            set => _Balance = value;
        }

        private int _Outstanding;

        public int Outstanding
        {
            get => _Outstanding;
            set => _Outstanding = value;
        }

        private string _BalanceDetail;

        public string BalanceDetail
        {
            get => _BalanceDetail;
            set => _BalanceDetail = value;
        }
    }
}