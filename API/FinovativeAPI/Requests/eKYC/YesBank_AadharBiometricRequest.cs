using Project.Processing;

namespace Project.Requests.eKYC
{
    public class YesBank_AadharBiometricRequest
    {
        private string _RequestID;

        public string RequestID
        {
            get => _RequestID.TrimIfNotNull();
            set => _RequestID = value.TrimIfNotNull();
        }
        private string _FingerPrint;

        public string FingerPrint
        {
            get => _FingerPrint.TrimIfNotNull();
            set => _FingerPrint = value.TrimIfNotNull();
        }
        private string _KYCToken;

        public string KYCToken
        {
            get => _KYCToken.TrimIfNotNull();
            set => _KYCToken = value.TrimIfNotNull();
        }
        private string _SerialNumber;

        public string SerialNumber
        {
            get => _SerialNumber.TrimIfNotNull();
            set => _SerialNumber = value.TrimIfNotNull();
        }
        private string _BiometricType;

        public string BiometricType
        {
            get => _BiometricType.TrimIfNotNull();
            set => _BiometricType = value.TrimIfNotNull();
        }
        private string _AadharNo;

        public string AadharNo
        {
            get => _AadharNo.TrimIfNotNull();
            set => _AadharNo = value.TrimIfNotNull();
        }
        private string _WadhValue;

        public string WadhValue
        {
            get => _WadhValue.TrimIfNotNull();
            set => _WadhValue = value.TrimIfNotNull();
        }
    }
}
