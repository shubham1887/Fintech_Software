using Newtonsoft.Json;
using YESBankCore.Controller;

namespace YesBankCore.Response.Payout
{
    public class PennyVerificationAPIResponse
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
        private string _partnerReferenceNumber;
       
        public string partnerReferenceNumber
        {
            get => _partnerReferenceNumber.TrimIfNotNull();
            set => _partnerReferenceNumber = value.TrimIfNotNull();
        }
        private string _beneficiarName;
       
        public string beneficiarName
        {
            get => _beneficiarName.TrimIfNotNull();
            set => _beneficiarName = value.TrimIfNotNull();
        }
        private string _rrn;
       
        public string rrn
        {
            get => _rrn.TrimIfNotNull();
            set => _rrn = value.TrimIfNotNull();
        }
    }
}
