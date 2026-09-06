using Project.Processing;

namespace Projecdt.Response.TransactionReport
{
    public class ProviderFailureViewModel
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        public string FailuerMessage
        {
            get => _FailuerMessage.TrimIfNotNull();
            set => _FailuerMessage = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }

        private string _DOC;
        public string DOC { get => _DOC.TrimIfNotNull(); set => _DOC = value.TrimIfNotNull(); }
        private string _FailuerMessage;
    }
}