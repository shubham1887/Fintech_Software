namespace Project.Response.User
{
    public class BalanceHeaderViewModel
    {
        private decimal _TotalRechargeBalance;

        public decimal TotalRechargeBalance
        {
            get => _TotalRechargeBalance;
            set => _TotalRechargeBalance = value;
        }

        private decimal _TotalDMTBalance;

        public decimal TotalDMTBalance
        {
            get => _TotalDMTBalance;
            set => _TotalDMTBalance = value;
        }

        private decimal _TotalAEPSBalance;

        public decimal TotalAEPSBalance
        {
            get => _TotalAEPSBalance;
            set => _TotalAEPSBalance = value;
        }

        private decimal _TotalBalance;

        public decimal TotalBalance
        {
            get => _TotalBalance;
            set => _TotalBalance = value;
        }

        private decimal _TotalOutstanding;

        public decimal TotalOutstanding
        {
            get => _TotalOutstanding;
            set => _TotalOutstanding = value;
        }
    }

    public class Headerlist
    {
        private string _name;

        public string name
        {
            get => _name;
            set => _name = value;
        }

        private decimal _amount;

        public decimal amount
        {
            get => _amount;
            set => _amount = value;
        }
    }
}