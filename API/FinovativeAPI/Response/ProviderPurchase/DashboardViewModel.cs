using Project.Processing;

namespace Project.Response.ProviderPurchase
{
    public class DashboardViewModel
    {
        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private decimal _Purchase;

        public decimal Purchase
        {
            get => _Purchase;
            set => _Purchase = value;
        }

        private decimal _Balance;

        public decimal Balance
        {
            get => _Balance;
            set => _Balance = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

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

        private string _Balances;

        public string Balances
        {
            get => _Balances;
            set => _Balances = value;
        }
    }
}