using Newtonsoft.Json;
using YESBankCore.Controller;

namespace YesBankCore.Response.Payout
{
    public class BalanceFetchAPIResponse
    {
        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }
        private string _responseCode;

        public string responseCode
        {
            get => _responseCode.TrimIfNotNull();
            set => _responseCode = value.TrimIfNotNull();
        }
        private string _responseMessage;

        public string responseMessage
        {
            get => _responseMessage.TrimIfNotNull();
            set => _responseMessage = value.TrimIfNotNull();
        }
        private string _balance;

        public string balance
        {
            get => _balance.TrimIfNotNull();
            set => _balance = value.TrimIfNotNull();
        }
    }
}
