using Project.Processing;

namespace Project.Response.LapuTransactionDetail
{
    public class UsageViewModel
    {
        private string _LapuNo;

        public string LapuNo
        {
            get => _LapuNo.TrimIfNotNull();
            set => _LapuNo = value.TrimIfNotNull();
        }

        private string _PartyName;

        public string PartyName
        {
            get => _PartyName.TrimIfNotNull();
            set => _PartyName = value.TrimIfNotNull();
        }

        private int _TotalTransaction;

        public int TotalTransaction
        {
            get => _TotalTransaction;
            set => _TotalTransaction = value;
        }

        private decimal _TotalTransactionAmount;

        public decimal TotalTransactionAmount
        {
            get => _TotalTransactionAmount;
            set => _TotalTransactionAmount = value;
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }
    }
}