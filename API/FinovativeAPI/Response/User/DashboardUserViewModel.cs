namespace Project.Response.User
{
    public class DashboardUserViewModel
    {
        private string _UserType;

        public string UserType
        {
            get => _UserType;
            set => _UserType = value;
        }

        private string _WalletName;

        public string WalletName
        {
            get => _WalletName;
            set => _WalletName = value;
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }

        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private decimal _Recharge;

        public decimal Recharge
        {
            get => _Recharge;
            set => _Recharge = value;
        }

        private decimal _DMT;

        public decimal DMT
        {
            get => _DMT;
            set => _DMT = value;
        }

        private decimal _AEPS;

        public decimal AEPS
        {
            get => _AEPS;
            set => _AEPS = value;
        }

        private decimal _TotalBalance;

        public decimal TotalBalance
        {
            get => _TotalBalance;
            set => _TotalBalance = value;
        }

        private float _WalletBalance;

        public float WalletBalance
        {
            get => _WalletBalance;
            set => _WalletBalance = value;
        }
    }
}