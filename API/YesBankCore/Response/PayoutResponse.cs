using Newtonsoft.Json;
using YESBankCore.Controller;

namespace YesBankCore.Response
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

        private string _BeneficiaryName;

        [JsonProperty("BeneficiaryName")]
        public string BeneficiaryName
        {
            get => _BeneficiaryName.TrimIfNotNull();
            set => _BeneficiaryName = value.TrimIfNotNull();
        }

        private string _AccountNumber;

        [JsonProperty("AccountNumber")]
        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }

        private string _Amount;

        [JsonProperty("Amount")]
        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _BankRRN;

        [JsonProperty("BankRRN")]
        public string BankRRN
        {
            get => _BankRRN.TrimIfNotNull();
            set => _BankRRN = value.TrimIfNotNull();
        }

        private string _RequestId;

        [JsonProperty("RequestId")]
        public string RequestId
        {
            get => _RequestId.TrimIfNotNull();
            set => _RequestId = value.TrimIfNotNull();
        }
    }
}