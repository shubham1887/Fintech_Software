namespace Project.Response.TransactionDetails
{
    public class AdminAccountingViewModel
    {
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

        private decimal _Transfer;

        public decimal Transfer
        {
            get => _Transfer;
            set => _Transfer = value;
        }

        private decimal _AEPSTransfer;

        public decimal AEPSTransfer
        {
            get => _AEPSTransfer;
            set => _AEPSTransfer = value;
        }

        private decimal _PGTransfer;

        public decimal PGTransfer
        {
            get => _PGTransfer;
            set => _PGTransfer = value;
        }

        private decimal _AEPSSuccess;

        public decimal AEPSSuccess
        {
            get => _AEPSSuccess;
            set => _AEPSSuccess = value;
        }

        private decimal _Reverse;

        public decimal Reverse
        {
            get => _Reverse;
            set => _Reverse = value;
        }

        private decimal _CommissionCredit;

        public decimal CommissionCredit
        {
            get => _CommissionCredit;
            set => _CommissionCredit = value;
        }

        private decimal _CommissionDebit;

        public decimal CommissionDebit
        {
            get => _CommissionDebit;
            set => _CommissionDebit = value;
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

        private decimal _ExpectedClosingBalance;

        public decimal ExpectedClosingBalance
        {
            get => _ExpectedClosingBalance;
            set => _ExpectedClosingBalance = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private decimal _Surcharge;

        public decimal Surcharge
        {
            get => _Surcharge;
            set => _Surcharge = value;
        }

        private decimal _Success;

        public decimal Success
        {
            get => _Success;
            set => _Success = value;
        }

        private decimal _FailedAmount;

        public decimal FailedAmount
        {
            get => _FailedAmount;
            set => _FailedAmount = value;
        }

        private decimal _PastFail;

        public decimal PastFail
        {
            get => _PastFail;
            set => _PastFail = value;
        }

        private decimal _WalletTransfer;

        public decimal WalletTransfer
        {
            get => _WalletTransfer;
            set => _WalletTransfer = value;
        }
    }
}