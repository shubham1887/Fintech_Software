using YESBankCore.Controller;

namespace YesBankCore.Request.AEPS
{
    internal class AuthenticationAPIRequest
    {
        private string _txnId;

        public string txnId
        {
            get => _txnId.TrimIfNotNull();
            set => _txnId = value.TrimIfNotNull();
        }

        private string _hashKey;

        public string hashKey
        {
            get => _hashKey.TrimIfNotNull();
            set => _hashKey = value.TrimIfNotNull();
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

        private string _aadharNumber;

        public string aadharNumber
        {
            get => _aadharNumber.TrimIfNotNull();
            set => _aadharNumber = value.TrimIfNotNull();
        }

        private string _isVid;

        public string isVid
        {
            get => _isVid.TrimIfNotNull();
            set => _isVid = value.TrimIfNotNull();
        }

        private string _txnType;

        public string txnType
        {
            get => _txnType.TrimIfNotNull();
            set => _txnType = value.TrimIfNotNull();
        }

        private string _biometricData;

        public string biometricData
        {
            get => _biometricData.TrimIfNotNull();
            set => _biometricData = value.TrimIfNotNull();
        }

        private string _customerConsent;

        public string customerConsent
        {
            get => _customerConsent.TrimIfNotNull();
            set => _customerConsent = value.TrimIfNotNull();
        }

        private string _latitude;

        public string latitude
        {
            get => _latitude.TrimIfNotNull();
            set => _latitude = value.TrimIfNotNull();
        }

        private string _longitude;

        public string longitude
        {
            get => _longitude.TrimIfNotNull();
            set => _longitude = value.TrimIfNotNull();
        }

        private string _appVersion;

        public string appVersion
        {
            get => _appVersion.TrimIfNotNull();
            set => _appVersion = value.TrimIfNotNull();
        }
        private string _appId;

        public string appId
        {
            get => _appId.TrimIfNotNull();
            set => _appId = value.TrimIfNotNull();
        }

        private string _agentName;

        public string agentName
        {
            get => _agentName.TrimIfNotNull();
            set => _agentName = value.TrimIfNotNull();
        }

        private string _agentAddress;

        public string agentAddress
        {
            get => _agentAddress.TrimIfNotNull();
            set => _agentAddress = value.TrimIfNotNull();
        }

        private string _agentPincode;

        public string agentPincode
        {
            get => _agentPincode.TrimIfNotNull();
            set => _agentPincode = value.TrimIfNotNull();
        }

        private string _agentDistrict;

        public string agentDistrict
        {
            get => _agentDistrict.TrimIfNotNull();
            set => _agentDistrict = value.TrimIfNotNull();
        }

        private string _agentState;

        public string agentState
        {
            get => _agentState.TrimIfNotNull();
            set => _agentState = value.TrimIfNotNull();
        }

        private string _agentCountry;

        public string agentCountry
        {
            get => _agentCountry.TrimIfNotNull();
            set => _agentCountry = value.TrimIfNotNull();
        }

        private string _deviceType;

        public string deviceType
        {
            get => _deviceType.TrimIfNotNull();
            set => _deviceType = value.TrimIfNotNull();
        }

        private string _deviceMake;

        public string deviceMake
        {
            get => _deviceMake.TrimIfNotNull();
            set => _deviceMake = value.TrimIfNotNull();
        }

        private string _deviceModel;

        public string deviceModel
        {
            get => _deviceModel.TrimIfNotNull();
            set => _deviceModel = value.TrimIfNotNull();
        }

        private string _deviceIp;

        public string deviceIp
        {
            get => _deviceIp.TrimIfNotNull();
            set => _deviceIp = value.TrimIfNotNull();
        }
        private string _authType;

        public string authType
        {
            get => _authType.TrimIfNotNull();
            set => _authType = value.TrimIfNotNull();
        }
    }
}
