using Project.Processing;

namespace Project.Response.Performance
{
    public class PerformanceViewModel
    {
        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private string _ClientName;

        public string ClientName
        {
            get => _ClientName.TrimIfNotNull();
            set => _ClientName = value.TrimIfNotNull();
        }

        private string _TxnDoc;

        public string TxnDoc
        {
            get => _TxnDoc.TrimIfNotNull();
            set => _TxnDoc = value.TrimIfNotNull();
        }

        private decimal _SuccessAmount;

        public decimal SuccessAmount
        {
            get => _SuccessAmount;
            set => _SuccessAmount = value;
        }

        private int _SuccessCount;

        public int SuccessCount
        {
            get => _SuccessCount;
            set => _SuccessCount = value;
        }

        private decimal _FailAmount;

        public decimal FailAmount
        {
            get => _FailAmount;
            set => _FailAmount = value;
        }

        private int _FailCount;

        public int FailCount
        {
            get => _FailCount;
            set => _FailCount = value;
        }
    }
}