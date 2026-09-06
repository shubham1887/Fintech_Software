using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    internal class BeneAddAPIRequest
    {
        private string _txnId;

        public string txnId
        {
            get => _txnId.TrimIfNotNull();
            set => _txnId = value.TrimIfNotNull();
        }

        private string _customerMob;

        public string customerMob
        {
            get => _customerMob.TrimIfNotNull();
            set => _customerMob = value.TrimIfNotNull();
        }

        private string _agentId;

        public string agentId
        {
            get => _agentId.TrimIfNotNull();
            set => _agentId = value.TrimIfNotNull();
        }

        private string _partnerId;

        public string partnerId
        {
            get => _partnerId.TrimIfNotNull();
            set => _partnerId = value.TrimIfNotNull();
        }

        private string _hashKey;

        public string hashKey
        {
            get => _hashKey.TrimIfNotNull();
            set => _hashKey = value.TrimIfNotNull();
        }

        private string _channel;

        public string channel
        {
            get => _channel.TrimIfNotNull();
            set => _channel = value.TrimIfNotNull();
        }

        private string _benefAccNo;

        public string benefAccNo
        {
            get => _benefAccNo.TrimIfNotNull();
            set => _benefAccNo = value.TrimIfNotNull();
        }

        private string _benefIfscCode;

        public string benefIfscCode
        {
            get => _benefIfscCode.TrimIfNotNull();
            set => _benefIfscCode = value.TrimIfNotNull();
        }

        private string _name;

        public string name
        {
            get => _name.TrimIfNotNull();
            set => _name = value.TrimIfNotNull();
        }

        private string _title;

        public string title
        {
            get => _title.TrimIfNotNull();
            set => _title = value.TrimIfNotNull();
        }

        //public List<string> productCode { get; set; }

        private string _productCode;

        public string productCode
        {
            get => _productCode.TrimIfNotNull();
            set => _productCode = value.TrimIfNotNull();
        }

        private string _address1;

        public string address1
        {
            get => _address1.TrimIfNotNull();
            set => _address1 = value.TrimIfNotNull();
        }

        private string _pincode;

        public string pincode
        {
            get => _pincode.TrimIfNotNull();
            set => _pincode = value.TrimIfNotNull();
        }

        private string _city;

        public string city
        {
            get => _city.TrimIfNotNull();
            set => _city = value.TrimIfNotNull();
        }

        private string _country;

        public string country
        {
            get => _country.TrimIfNotNull();
            set => _country = value.TrimIfNotNull();
        }

        private string _otp;

        public string otp
        {
            get => _otp.TrimIfNotNull();
            set => _otp = value.TrimIfNotNull();
        }
    }
}
