using Project.Processing;

namespace Project.Response.PaytmCCFAPIResponse
{
    public class SenderRegistrationGenerateOTPResponse
    {
        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }
        

        private string _message;
        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }
                

        private int _responseCode;
        public int responseCode
        {
            get => _responseCode;
            set => _responseCode = value;
        }

        private string _State;
        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

    }
}