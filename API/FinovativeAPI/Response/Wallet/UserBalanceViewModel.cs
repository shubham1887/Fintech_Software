namespace Project.Response.Wallet
{
    public class UserBalanceViewModel
    {
        private decimal _RechargeBalance;

        public decimal RechargeBalance
        {
            get => _RechargeBalance;
            set => _RechargeBalance = value;
        }

        private decimal _DMTBalance;

        public decimal DMTBalance
        {
            get => _DMTBalance;
            set => _DMTBalance = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
    }
}