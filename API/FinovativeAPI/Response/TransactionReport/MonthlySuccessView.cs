namespace Project.Response.TransactionReport
{
    public class MonthlySuccessView
    {
        private int _DOC;

        public int DOC
        {
            get => _DOC;
            set => _DOC = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _DistributerUserID;

        public int DistributerUserID
        {
            get => _DistributerUserID;
            set => _DistributerUserID = value;
        }

        private int _SuperDistributerUserID;

        public int SuperDistributerUserID
        {
            get => _SuperDistributerUserID;
            set => _SuperDistributerUserID = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _GivenCommission;

        public decimal GivenCommission
        {
            get => _GivenCommission;
            set => _GivenCommission = value;
        }

        private decimal _AdminCommission;

        public decimal AdminCommission
        {
            get => _AdminCommission;
            set => _AdminCommission = value;
        }
    }
}