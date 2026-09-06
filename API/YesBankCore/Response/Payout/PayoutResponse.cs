using Newtonsoft.Json;
using YESBankCore.Controller;

namespace YesBankCore.Response.Payout
{
    public class PayoutResponse
    {
        private string _Status;

        [JsonProperty("Status")]
        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        [JsonProperty("Message")]
        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
        private string _BankRRN;

        [JsonProperty("BankRRN")]
        public string BankRRN
        {
            get => _BankRRN.TrimIfNotNull();
            set => _BankRRN = value.TrimIfNotNull();
        }

        private string _ReferenceNo;

        [JsonProperty("ReferenceNo")]
        public string ReferenceNo
        {
            get => _ReferenceNo.TrimIfNotNull();
            set => _ReferenceNo = value.TrimIfNotNull();
        }
        private string _RequestId;

        [JsonProperty("RequestId")]
        public string RequestId
        {
            get => _RequestId.TrimIfNotNull();
            set => _RequestId = value.TrimIfNotNull();
        }
        private string _BeneficiarName;

        [JsonProperty("BeneficiarName")]
        public string BeneficiarName
        {
            get => _BeneficiarName.TrimIfNotNull();
            set => _BeneficiarName = value.TrimIfNotNull();
        }
    }
}
