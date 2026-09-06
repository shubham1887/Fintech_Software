using Project.Processing;

namespace Project.Response.PaytmCCFAPIResponse
{
    public class BalanceAPIResponse
    {

        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _accountNumber;
        public string accountNumber
        {
            get => _accountNumber.TrimIfNotNull();
            set => _accountNumber = value.TrimIfNotNull();
        }

        private string _message;
        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }

        private decimal _effectiveBalance;
        public decimal effectiveBalance
        {
            get => _effectiveBalance;
            set => _effectiveBalance = value;
        }
      
    }
}
