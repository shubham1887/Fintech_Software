namespace Project.Response.UserwiseSaleTarget
{
    public class MonthlySuccessViewModel
    {
        private int _DOC;

        public int DOC
        {
            get => _DOC;
            set => _DOC = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
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

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }
    }
}