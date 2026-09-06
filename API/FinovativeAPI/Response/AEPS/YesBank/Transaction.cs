
using Project.Processing;

namespace Project.Response.AEPS
{
    public class Transaction
    {
        private string _Date;

        public string Date
        {
            get => _Date.TrimIfNotNull();
            set => _Date = value.TrimIfNotNull();
        }

        private string _TxnType;

        public string TxnType
        {
            get => _TxnType.TrimIfNotNull();
            set => _TxnType = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Narration;

        public string Narration
        {
            get => _Narration.TrimIfNotNull();
            set => _Narration = value.TrimIfNotNull();
        }
    }
}
