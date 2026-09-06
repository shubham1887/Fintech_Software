using Project.Processing;

namespace Project.Requests.IMoneyPay
{
    public class StatusCheckRequest
    {
        private string _PAY_ID;

        public string PAY_ID
        {
            get => _PAY_ID.TrimIfNotNull();
            set => _PAY_ID = value.TrimIfNotNull();
        }

        private string _ORDER_ID;

        public string ORDER_ID
        {
            get => _ORDER_ID.TrimIfNotNull();
            set => _ORDER_ID = value.TrimIfNotNull();
        }

        private string _AMOUNT;

        public string AMOUNT
        {
            get => _AMOUNT.TrimIfNotNull();
            set => _AMOUNT = value.TrimIfNotNull();
        }

        public string TXNTYPE = "STATUS";
        public string CURRENCY_CODE = "356";

        private string _HASH;

        public string HASH
        {
            get => _HASH.TrimIfNotNull();
            set => _HASH = value.TrimIfNotNull();
        }
    }
}