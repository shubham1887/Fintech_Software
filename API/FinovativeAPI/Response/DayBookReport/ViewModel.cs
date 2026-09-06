using Project.Processing;

namespace Project.Response.DayBookReport
{
    public class ViewModel
    {
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

        private int _TotalCount;

        public int TotalCount
        {
            get => _TotalCount;
            set => _TotalCount = value;
        }

        private decimal _TotalAmount;

        public decimal TotalAmount
        {
            get => _TotalAmount;
            set => _TotalAmount = value;
        }

        private int _SuccessCount;

        public int SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
        }

        private decimal _SuccessAmount;

        public decimal SuccessAmount
        {
            get => _SuccessAmount;
            set => _SuccessAmount = value;
        }

        private int _FailedCount;

        public int FailedCount
        {
            get => _FailedCount;
            set => _FailedCount = value;
        }

        private decimal _FailedAmount;

        public decimal FailedAmount
        {
            get => _FailedAmount;
            set => _FailedAmount = value;
        }

        private int _PendingCount;

        public int PendingCount
        {
            get => _PendingCount;
            set => _PendingCount = value;
        }

        private decimal _PendingAmount;

        public decimal PendingAmount
        {
            get => _PendingAmount;
            set => _PendingAmount = value;
        }

        private decimal _SuperDistributerCommission;

        public decimal SuperDistributerCommission
        {
            get => _SuperDistributerCommission;
            set => _SuperDistributerCommission = value;
        }

        private decimal _DistributerCommission;

        public decimal DistributerCommission
        {
            get => _DistributerCommission;
            set => _DistributerCommission = value;
        }

        private decimal _RetailerCommission;

        public decimal RetailerCommission
        {
            get => _RetailerCommission;
            set => _RetailerCommission = value;
        }

        private decimal _Surcharge;

        public decimal Surcharge
        {
            get => _Surcharge;
            set => _Surcharge = value;
        }

        private decimal _AdminCommission;

        public decimal AdminCommission
        {
            get => _AdminCommission;
            set => _AdminCommission = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }
    }
}