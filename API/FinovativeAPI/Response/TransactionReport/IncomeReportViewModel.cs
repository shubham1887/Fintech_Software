namespace Project.Response.TransactionReport
{
    public class IncomeReportViewModel
    {
        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName;
            set => _ServiceName = value;
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
        private decimal _AdminCommission;

        public decimal AdminCommission
        {
            get => _AdminCommission;
            set => _AdminCommission = value;
        }
        private decimal _NetProfit;

        public decimal NetProfit
        {
            get => _NetProfit;
            set => _NetProfit = value;
        }

        
    }
}