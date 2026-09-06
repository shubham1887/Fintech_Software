using Project.Processing;

namespace Project.Requests.TransactionReport
{
    public class UserSaleViewModel
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

        public decimal OpeningBal { get; internal set; }

        private decimal _ClosingBalance;

        public decimal ClosingBalance
        {
            get => _ClosingBalance;
            set => _ClosingBalance = value;
        }

        private decimal _Sale;

        public decimal Sale
        {
            get => _Sale;
            set => _Sale = value;
        }

        private decimal _RetailerCommission;

        public decimal RetailerCommission
        {
            get => _RetailerCommission;
            set => _RetailerCommission = value;
        }

        public decimal DistributerCommission { get; internal set; }
        public decimal SuperDistributerCommission { get; internal set; }

        private decimal _Surcharge;

        public decimal Surcharge
        {
            get => _Surcharge;
            set => _Surcharge = value;
        }

        private decimal _Purchase;

        public decimal Purchase
        {
            get => _Purchase;
            set => _Purchase = value;
        }

        public decimal TransferAmount { get; internal set; }

        public decimal ReverseAmountDr { get; internal set; }

        public decimal ReverseAmountCr { get; internal set; }

        private int _SuccessCount;

        public int SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
        }

        private int _FailCount;

        public int FailCount
        {
            get => _FailCount;
            set => _FailCount = value;
        }

        private decimal _Difference;

        public decimal Difference
        {
            get => _Difference;
            set => _Difference = value;
        }

        private decimal _Reverse;

        public decimal Reverse
        {
            get => _Reverse;
            set => _Reverse = value;
        }

        private decimal _OpeningBalance;

        public decimal OpeningBalance
        {
            get => _OpeningBalance;
            set => _OpeningBalance = value;
        }

        public int UserID { get; internal set; }
        public decimal ComplaintAmount { get; internal set; }
        public decimal Requested { get; internal set; }
        public decimal Debited { get; internal set; }
        public decimal PastSuccessAmount { get; internal set; }
        public decimal PastFailAmount { get; internal set; }

        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
        }
    }
}