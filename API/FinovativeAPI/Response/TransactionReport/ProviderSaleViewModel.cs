using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class ProviderSaleViewModel
    {
        private string _ProvderName;

        public string ProvderName
        {
            get => _ProvderName.TrimIfNotNull();
            set => _ProvderName = value.TrimIfNotNull();
        }

        private decimal _OpeningBalance;

        public decimal OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
        }

        private decimal _Purchase;

        public decimal Purchase
        {
            get => _Purchase;
            set => _Purchase = value;
        }

        private decimal _Sale;

        public decimal Sale
        {
            get => _Sale;
            set => _Sale = value;
        }

        private decimal _RetailerCommission;

        public decimal RetailerCommission
        {
            get => _RetailerCommission;
            set => _RetailerCommission = value;
        }

        private decimal _DistributerCommission;

        public decimal DistributerCommission
        {
            get => _DistributerCommission;
            set => _DistributerCommission = value;
        }

        private decimal _SuperDistributerCommission;

        public decimal SuperDistributerCommission
        {
            get => _SuperDistributerCommission;
            set => _SuperDistributerCommission = value;
        }

        private decimal _AdminCommission;

        public decimal AdminCommission
        {
            get => _AdminCommission;
            set => _AdminCommission = value;
        }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }

        private decimal _Difference;

        public decimal Difference
        {
            get => _Difference;
            set => _Difference = value;
        }

        private decimal _Profit;

        public decimal Profit
        {
            get => _Profit;
            set => _Profit = value;
        }

        private decimal _Fail;

        public decimal Fail
        {
            get => _Fail;
            set => _Fail = value;
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }
    }
}