namespace Project.Response.TransactionReport
{
    public class SaleViewModel
    {
        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName;
            set => _ServiceName = value;
        }

        private int _FailCount;

        public int FailCount
        {
            get => _FailCount;
            set => _FailCount = value;
        }

        private int _SuccessCount;

        public int SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
        }

        private decimal _SuccessAmt;

        public decimal SuccessAmt
        {
            get => _SuccessAmt;
            set => _SuccessAmt = value;
        }

        private decimal _FailAmt;

        public decimal FailAmt
        {
            get => _FailAmt;
            set => _FailAmt = value;
        }

        private decimal _RCommission;

        public decimal RCommission
        {
            get => _RCommission;
            set => _RCommission = value;
        }

        private decimal _DCommission;

        public decimal DCommission
        {
            get => _DCommission;
            set => _DCommission = value;
        }

        private decimal _SCommission;

        public decimal SCommission
        {
            get => _SCommission;
            set => _SCommission = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private decimal _PendingAmount;

        public decimal PendingAmount
        {
            get => _PendingAmount;
            set => _PendingAmount = value;
        }

        private int _PendingCount;

        public int PendingCount
        {
            get => _PendingCount;
            set => _PendingCount = value;
        }

        private decimal _ACommission;

        public decimal ACommission
        {
            get => _ACommission;
            set => _ACommission = value;
        }
    }
}