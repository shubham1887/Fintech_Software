using Project.Processing;

namespace Project.Response.TransactionReport
{
    public class ProviderFailureGraphViewModel
    {
        private int _DOC;

        public int DOC
        {
            get => _DOC;
            set => _DOC = value;
        }

        private string _DOCStr;

        public string DOCStr
        {
            get => _DOCStr.TrimIfNotNull();
            set => _DOCStr = value.TrimIfNotNull();
        }

        private string _FailureMessage;

        public string FailureMessage
        {
            get => _FailureMessage.TrimIfNotNull();
            set => _FailureMessage = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
    }
}