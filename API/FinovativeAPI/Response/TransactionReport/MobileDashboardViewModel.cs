namespace Project.Response.TransactionReport
{
    public class MobileDashboardViewModel
    {
        private int _TotalHits;

        public int TotalCount
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

        public int SuccessCount
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

        public int FailedCount
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

        public int PendingCount
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

        private decimal _DirectCommission;

        public decimal DirectCommission
        {
            get => _DirectCommission;
            set => _DirectCommission = value;
        }

        private decimal _RetailerCommission;

        public decimal RetailerCommission
        {
            get => _RetailerCommission;
            set => _RetailerCommission = value;
        }
    }
}