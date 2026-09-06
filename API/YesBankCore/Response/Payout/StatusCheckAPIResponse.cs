using Newtonsoft.Json;
using YESBankCore.Controller;

namespace YesBankCore.Response.Payout
{
    public class StatusCheckAPIResponse
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

        private string _txnStatus;
      
        public string txnStatus
        {
            get => _txnStatus.TrimIfNotNull();
            set => _txnStatus = value.TrimIfNotNull();
        }
        private string _responseMessage;
       
        public string responseMessage
        {
            get => _responseMessage.TrimIfNotNull();
            set => _responseMessage = value.TrimIfNotNull();
        }

        private string _partnerReferenceNumber;
     
        public string partnerReferenceNumber
        {
            get => _partnerReferenceNumber.TrimIfNotNull();
            set => _partnerReferenceNumber = value.TrimIfNotNull();
        }

        private string _yppTxnReferenceNumber;
       
        public string yppTxnReferenceNumber
        {
            get => _yppTxnReferenceNumber.TrimIfNotNull();
            set => _yppTxnReferenceNumber = value.TrimIfNotNull();
        }

        private string _channelReferenceNumber;

        public string channelReferenceNumber
        {
            get => _channelReferenceNumber.TrimIfNotNull();
            set => _channelReferenceNumber = value.TrimIfNotNull();
        }
        
    }
}
