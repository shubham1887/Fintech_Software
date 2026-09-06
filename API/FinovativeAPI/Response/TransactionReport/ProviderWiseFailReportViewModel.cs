namespace Project.Response.TransactionReport
{
    public class ProviderWiseFailReportViewModel
    {
        private string _Provider;

        public string Provider
        {
            get => _Provider;
            set => _Provider = value;
        }

        private string _FailMessage;

        public string FailMessage
        {
            get => _FailMessage;
            set => _FailMessage = value;
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
    }
}