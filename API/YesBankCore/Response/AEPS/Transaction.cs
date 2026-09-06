using YESBankCore.Controller;

namespace YesBankCore.Response.AEPS
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

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _Narration;

        public string Narration
        {
            get => _Narration.TrimIfNotNull();
            set => _Narration = value.TrimIfNotNull();
        }
    }
}
