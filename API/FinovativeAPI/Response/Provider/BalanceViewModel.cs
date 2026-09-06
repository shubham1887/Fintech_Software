namespace Project.Response.Provider
{
    public class BalanceViewModel
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
    }
}