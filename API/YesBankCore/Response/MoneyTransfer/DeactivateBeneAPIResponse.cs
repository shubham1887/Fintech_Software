using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    internal class DeactivateBeneAPIResponse
    {
        private string _responseCode;

        public string responseCode
        {
            get => _responseCode.TrimIfNotNull();
            set => _responseCode = value.TrimIfNotNull();
        }

        private string _responseMsg;

        public string responseMsg
        {
            get => _responseMsg.TrimIfNotNull();
            set => _responseMsg = value.TrimIfNotNull();
        }
       
    }
}
