using Project.Processing;

namespace Project.Response.Overdraft
{
    public class RequestViewModel
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
            get => dOC;
            set => dOC = value;
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

        public decimal SenderBalance { get; internal set; }
        public decimal OverdraftBalance { get; internal set; }
    }
}