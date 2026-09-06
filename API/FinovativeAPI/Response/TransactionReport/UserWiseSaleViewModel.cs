using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class UserWiseSaleViewModel
    {
        public int WalletID { get; internal set; }

        public int UserID { get; internal set; }

        public int UserTypeID { get; internal set; }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        public decimal Amount { get; internal set; }

        public decimal RetailerCommission { get; internal set; }

        public int SuperDistributerUserID { get; internal set; }
        public int DistributerUserID { get; internal set; }
        public decimal DistributerCommission { get; internal set; }
        public decimal SuperDistributerCommission { get; internal set; }
        public decimal Surcharge { get; internal set; }

        private int _DOCInt;
        public int DOCInt
        {
            get => _DOCInt;
            set => _DOCInt = value;
        }
    }
}