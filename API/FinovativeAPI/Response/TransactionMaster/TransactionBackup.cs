using Project.Processing;

namespace Project.Response.TransactionMaster
{
    public class TransactionBackup
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _DateTime;

        public string DateTime
        {
            get => _DateTime.TrimIfNotNull();
            set => _DateTime = value.TrimIfNotNull();
        }

        private string _Service;

        public string Service
        {
            get => _Service.TrimIfNotNull();
            set => _Service = value.TrimIfNotNull();
        }

        private string _Operator;

        public string Operator
        {
            get => _Operator.TrimIfNotNull();
            set => _Operator = value.TrimIfNotNull();
        }

        private string _Wallet;

        public string Wallet
        {
            get => _Wallet.TrimIfNotNull();
            set => _Wallet = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private decimal _OpeningBal;

        public decimal OpeningBal
        {
            get => _OpeningBal;
            set => _OpeningBal = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
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

        private decimal _ClosingBal;

        public decimal ClosingBal
        {
            get => _ClosingBal;
            set => _ClosingBal = value;
        }

        public int UserID { get; internal set; }
    }
}