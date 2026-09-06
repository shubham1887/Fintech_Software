namespace FinovativeAPI.Response.Dashboard
{
    public class DashboardDatewiseData
    {
        private int _TransactionMasterID;

        public int TransactionMasterID
        {
            get => _TransactionMasterID;
            set => _TransactionMasterID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status;
            set => _Status = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _DebitAmount;

        public decimal DebitAmount
        {
            get => _DebitAmount;
            set => _DebitAmount = value;
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

        private int _DOCInt;

        public int DOCInt
        {
            get => _DOCInt;
            set => _DOCInt = value;
        }

        private decimal _ROfferCommission;

        public decimal ROfferCommission
        {
            get => _ROfferCommission;
            set => _ROfferCommission = value;
        }

        private decimal _RetailerTDS;

        public decimal RetailerTDS
        {
            get => _RetailerTDS;
            set => _RetailerTDS = value;
        }

        private decimal _DistributerTDS;

        public decimal DistributerTDS
        {
            get => _DistributerTDS;
            set => _DistributerTDS = value;
        }

        private decimal _SuperDistributerTDS;

        public decimal SuperDistributerTDS
        {
            get => _SuperDistributerTDS;
            set => _SuperDistributerTDS = value;
        }
    }
}