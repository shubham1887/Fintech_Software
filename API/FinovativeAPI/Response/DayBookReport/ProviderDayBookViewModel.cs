using Project.Processing;

namespace Project.Response.DayBookReport
{
    public class ProviderDayBookViewModel
    {
        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }

        private int _TotalHits;

        public int TotalHits
        {
            get => _TotalHits;
            set => _TotalHits = value;
        }

        private decimal _TotalAmount;

        public decimal TotalAmount
        {
            get => _TotalAmount;
            set => _TotalAmount = value;
        }

        private int _SuccessHits;

        public int SuccessHits
        {
            get => _SuccessHits;
            set => _SuccessHits = value;
        }

        private decimal _SuccessAmount;

        public decimal SuccessAmount
        {
            get => _SuccessAmount;
            set => _SuccessAmount = value;
        }

        private int _FailedHits;

        public int FailedHits
        {
            get => _FailedHits;
            set => _FailedHits = value;
        }

        private decimal _FailedAmount;

        public decimal FailedAmount
        {
            get => _FailedAmount;
            set => _FailedAmount = value;
        }

        private int _PendingHits;

        public int PendingHits
        {
            get => _PendingHits;
            set => _PendingHits = value;
        }

        private decimal _PendingAmount;

        public decimal PendingAmount
        {
            get => _PendingAmount;
            set => _PendingAmount = value;
        }

        private decimal _ProviderCommission;

        public decimal ProviderCommission
        {
            get => _ProviderCommission;
            set => _ProviderCommission = value;
        }

        private decimal _AdminCommission;

        public decimal AdminCommission
        {
            get => _AdminCommission;
            set => _AdminCommission = value;
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

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private decimal _Profit;

        public decimal Profit
        {
            get => _Profit;
            set => _Profit = value;
        }

        private decimal _Percentage;

        public decimal Percentage
        {
            get => _Percentage;
            set => _Percentage = value;
        }
    }
}