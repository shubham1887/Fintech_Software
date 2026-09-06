using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    internal class SenderAddAPIRequest
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

        private string _name;

        public string name
        {
            get => _name.TrimIfNotNull();
            set => _name = value.TrimIfNotNull();
        }

        private string _address;

        public string address
        {
            get => _address.TrimIfNotNull();
            set => _address = value.TrimIfNotNull();
        }

        private string _dateOfBirth;

        public string dateOfBirth
        {
            get => _dateOfBirth.TrimIfNotNull();
            set => _dateOfBirth = value.TrimIfNotNull();
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

        private string _kycStatus;

        public string kycStatus
        {
            get => _kycStatus.TrimIfNotNull();
            set => _kycStatus = value.TrimIfNotNull();
        }

        private string _pancardStatus;

        public string pancardStatus
        {
            get => _pancardStatus.TrimIfNotNull();
            set => _pancardStatus = value.TrimIfNotNull();
        }

        private string _identityDocType;

        public string identityDocType
        {
            get => _identityDocType.TrimIfNotNull();
            set => _identityDocType = value.TrimIfNotNull();
        }

        private string _identityDocNo;

        public string identityDocNo
        {
            get => _identityDocNo.TrimIfNotNull();
            set => _identityDocNo = value.TrimIfNotNull();
        }

        private string _addressDocType;

        public string addressDocType
        {
            get => _addressDocType.TrimIfNotNull();
            set => _addressDocType = value.TrimIfNotNull();
        }

        private string _addressDocNo;

        public string addressDocNo
        {
            get => _addressDocNo.TrimIfNotNull();
            set => _addressDocNo = value.TrimIfNotNull();
        }

        private string _cardStatus;

        public string cardStatus
        {
            get => _cardStatus.TrimIfNotNull();
            set => _cardStatus = value.TrimIfNotNull();
        }

        private string _city;

        public string city
        {
            get => _city.TrimIfNotNull();
            set => _city = value.TrimIfNotNull();
        }

        private string _gender;

        public string gender
        {
            get => _gender.TrimIfNotNull();
            set => _gender = value.TrimIfNotNull();
        }

        private string _title;

        public string title
        {
            get => _title.TrimIfNotNull();
            set => _title = value.TrimIfNotNull();
        }

        private string _pincode;

        public string pincode
        {
            get => _pincode.TrimIfNotNull();
            set => _pincode = value.TrimIfNotNull();
        }

        private string _district;

        public string district
        {
            get => _district.TrimIfNotNull();
            set => _district = value.TrimIfNotNull();
        }

        private string _state;

        public string state
        {
            get => _state.TrimIfNotNull();
            set => _state = value.TrimIfNotNull();
        }

        public List<string> productCode { get; set; }

        private string _versionNo;

        public string versionNo
        {
            get => _versionNo.TrimIfNotNull();
            set => _versionNo = value.TrimIfNotNull();
        }

        private string _channel;

        public string channel
        {
            get => _channel.TrimIfNotNull();
            set => _channel = value.TrimIfNotNull();
        }

        private string _isOTPValidated;

        public string isOTPValidated
        {
            get => _isOTPValidated.TrimIfNotNull();
            set => _isOTPValidated = value.TrimIfNotNull();
        }

        private string _maritalStatus;

        public string maritalStatus
        {
            get => _maritalStatus.TrimIfNotNull();
            set => _maritalStatus = value.TrimIfNotNull();
        }

        private string _hashKey;

        public string hashKey
        {
            get => _hashKey.TrimIfNotNull();
            set => _hashKey = value.TrimIfNotNull();
        }
    }
}
