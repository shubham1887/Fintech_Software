using Project.Processing;

namespace Project.Response.PaytmCCFAPIResponse
{
    public class SelectSenderAPIResponse
    {
        private string _Message;
        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private int _response_code;
        public int response_code
        {
            get => _response_code;
            set => _response_code = value;
        }

        private string _firstName;
        public string firstName
        {
            get => _firstName.TrimIfNotNull();
            set => _firstName = value.TrimIfNotNull();
        }

        private string _lastName;
        public string lastName
        {
            get => _lastName.TrimIfNotNull();
            set => _lastName = value.TrimIfNotNull();
        }

        private string _MobileNumber;
        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private decimal _RemainingLimit;
        public decimal RemainingLimit
        {
            get => _RemainingLimit;
            set => _RemainingLimit = value;
        }

   
        private string _errorMessage;
        public string errorMessage
        {
            get => _errorMessage.TrimIfNotNull();
            set => _errorMessage = value.TrimIfNotNull();
        }

        private string _otpGuid;
        public string otpGuid
        {
            get => _otpGuid.TrimIfNotNull();
            set => _otpGuid = value.TrimIfNotNull();
        }
    }
}