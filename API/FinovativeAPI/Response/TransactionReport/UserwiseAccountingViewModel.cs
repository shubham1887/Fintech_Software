using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class UserwiseAccountingViewModel
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _UserMobile;

        public string UserMobile
        {
            get => _UserMobile.TrimIfNotNull();
            set => _UserMobile = value.TrimIfNotNull();
        }

        private string _Role;

        public string Role
        {
            get => _Role.TrimIfNotNull();
            set => _Role = value.TrimIfNotNull();
        }

        private string _WalletName;

        public string WalletName
        {
            get => _WalletName.TrimIfNotNull();
            set => _WalletName = value.TrimIfNotNull();
        }


        private decimal _OpeningBal;

        public decimal OpeningBal
        {
            get => _OpeningBal;
            set => _OpeningBal = value;
        }

        private decimal _Purchase;

        public decimal Purchase
        {
            get => _Purchase;
            set => _Purchase = value;
        }
        private decimal _TotalOldAmountAdjustment;

        public decimal TotalOldAmountAdjustment
        {
            get => _TotalOldAmountAdjustment;
            set => _TotalOldAmountAdjustment = value;
        }

        private decimal _TotalNewAmountAdjustment;

        public decimal TotalNewAmountAdjustment
        {
            get => _TotalNewAmountAdjustment;
            set => _TotalNewAmountAdjustment = value;
        }
        private decimal _TotalCrAmt;

        public decimal TotalCrAmt
        {
            get => _TotalCrAmt;
            set => _TotalCrAmt = value;
        }

        private decimal _Sale;

        public decimal Sale
        {
            get => _Sale;
            set => _Sale = value;
        }
      
        private decimal _CommsionAmount;

        public decimal CommsionAmount
        {
            get => _CommsionAmount;
            set => _CommsionAmount = value;
        }

        private decimal _Surcharge;

        public decimal Surcharge
        {
            get => _Surcharge;
            set => _Surcharge = value;
        }
        private decimal _TotalDrAmt;

        public decimal TotalDrAmt
        {
            get => _TotalDrAmt;
            set => _TotalDrAmt = value;
        }
        private int _ManualUpdateCount;

        public int ManualUpdateCount
        {
            get => _ManualUpdateCount;
            set => _ManualUpdateCount = value;
        }
        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }
        private decimal _ExpClosingBal;

        public decimal ExpClosingBal
        {
            get => _ExpClosingBal;
            set => _ExpClosingBal = value;
        }
        private decimal _Difference;

        public decimal Difference
        {
            get => _Difference;
            set => _Difference = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

       





        

    }
}
