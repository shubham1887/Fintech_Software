using Project.Processing;

namespace Project.Response.Fund
{
    public class FundReceiverViewModel
    {
        public int DOCInt { get; internal set; }
        public decimal Amount { get; internal set; }

        public int SenderID { get; internal set; }

        public int WalletID { get; internal set; }

        public int ParentID { get; internal set; }
        public int ReceiverID { get; internal set; }

        public int SuperDistID { get; internal set; }
        public int UserTypeID { get; internal set; }

        private string _Through;

        public string Through
        {
            get => _Through.TrimIfNotNull();
            set => _Through = value.TrimIfNotNull();
        }
    }
}