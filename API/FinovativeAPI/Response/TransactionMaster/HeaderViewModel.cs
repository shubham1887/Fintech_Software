namespace Project.Response.TransactionMaster
{
    public class HeaderViewModel
    {
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

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private decimal _ManualFailAmount;

        public decimal ManualFailAmount
        {
            get => _ManualFailAmount;
            set => _ManualFailAmount = value;
        }

        private int _ManualFailCont;

        public int ManualFailCont
        {
            get => _ManualFailCont;
            set => _ManualFailCont = value;
        }

        private int _LossCount;

        public int LossCount
        {
            get => _LossCount;
            set => _LossCount = value;
        }

        private decimal _LossAmount;

        public decimal LossAmount
        {
            get => _LossAmount;
            set => _LossAmount = value;
        }

        private int _GrowthCount;

        public int GrowthCount
        {
            get => _GrowthCount;
            set => _GrowthCount = value;
        }

        private decimal _GrowthAmount;

        public decimal GrowthAmount
        {
            get => _GrowthAmount;
            set => _GrowthAmount = value;
        }   

    }
}