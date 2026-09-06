using Project.Processing;

namespace Project.Response.Overdraft
{
    public class ViewModel
    {
        private int _OverdraftDetailID;

        public int OverdraftDetailID
        {
            get => _OverdraftDetailID;
            set => _OverdraftDetailID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string dOC;

        public string DOC
        {
            get => dOC.TrimIfNotNull();
            set => dOC = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private decimal amount;

        public decimal Amount
        {
            get => amount;
            set => amount = value;
        }

        private string user;

        public string User
        {
            get => user.TrimIfNotNull();
            set => user = value.TrimIfNotNull();
        }

        private string wallet;

        public string Wallet
        {
            get => wallet.TrimIfNotNull();
            set => wallet = value.TrimIfNotNull();
        }

        private string userremark;

        public string UserRemark
        {
            get => userremark.TrimIfNotNull();
            set => userremark = value.TrimIfNotNull();
        }

        private string adminremark;

        public string AdminRemark
        {
            get => adminremark.TrimIfNotNull();
            set => adminremark = value.TrimIfNotNull();
        }

        private decimal _OverdraftOpening;

        public decimal OverdraftOpening
        {
            get => _OverdraftOpening;
            set => _OverdraftOpening = value;
        }

        private decimal _OverdraftClosing;

        public decimal OverdraftClosing
        {
            get => _OverdraftClosing;
            set => _OverdraftClosing = value;
        }

        private string _UpdatedDOC;

        public string UpdatedDOC
        {
            get => _UpdatedDOC.TrimIfNotNull();
            set => _UpdatedDOC = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }
    }
}