namespace Project.Response.Ledger
{
    public class LedgerHeader
    {
        private decimal _OpeningBal;

        public decimal OpeningBal
        {
            get { return _OpeningBal; }
            set { _OpeningBal = value; }
        }

        private decimal _TransferAmount;

        public decimal TransferAmount
        {
            get { return _TransferAmount; }
            set { _TransferAmount = value; }
        }

        private decimal _ReceiverAmount;

        public decimal ReceiverAmount
        {
            get => _ReceiverAmount;
            set => _ReceiverAmount = value;
        }

        private decimal _ReverseAmountCr;

        public decimal ReverseAmountCr
        {
            get { return _ReverseAmountCr; }
            set { _ReverseAmountCr = value; }
        }

        private decimal _ReverseAmountDr;

        public decimal ReverseAmountDr
        {
            get { return _ReverseAmountDr; }
            set { _ReverseAmountDr = value; }
        }

        private decimal _RefundAmount;

        public decimal RefundAmount
        {
            get { return _RefundAmount; }
            set { _RefundAmount = value; }
        }

        private decimal _CommissionAmount;

        public decimal CommissionAmount
        {
            get { return _CommissionAmount; }
            set { _CommissionAmount = value; }
        }

        private decimal _TDS;

        public decimal TDS
        {
            get { return _TDS; }
            set { _TDS = value; }
        }

        private decimal _Surcharge;

        public decimal Surcharge
        {
            get => _Surcharge;
            set => _Surcharge = value;
        }

        private decimal _Requested;

        public decimal Requested
        {
            get => _Requested;
            set => _Requested = value;
        }

        private decimal _Debited;

        public decimal Debited
        {
            get => _Debited;
            set => _Debited = value;
        }

        private decimal _ComplaintAmount;

        public decimal ComplaintAmount
        {
            get => _ComplaintAmount;
            set => _ComplaintAmount = value;
        }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }

        private decimal _ExpectedCloseBal;

        public decimal ExpectedCloseBal
        {
            get => _ExpectedCloseBal;
            set => _ExpectedCloseBal = value;
        }

        private decimal _Difference;

        public decimal Difference
        {
            get => _Difference;
            set => _Difference = value;
        }

        private decimal _PastFailAmount;

        public decimal PastFailAmount
        {
            get => _PastFailAmount;
            set => _PastFailAmount = value;
        }

        private decimal _PastSuccessAmount;

        public decimal PastSuccessAmount
        {
            get => _PastSuccessAmount;
            set => _PastSuccessAmount = value;
        }

        public decimal PGAmount { get; internal set; }
        public decimal ECollectionAmount { get; internal set; }
        public decimal AEPSAmount { get; internal set; }
        public decimal MATMAmount { get; internal set; }
    }
}