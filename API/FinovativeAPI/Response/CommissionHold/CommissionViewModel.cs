namespace Project.Response.CommissionHold
{
    public class CommissionViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private decimal _Commission;

        public decimal Commission
        {
            get => _Commission;
            set => _Commission = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
    }
}