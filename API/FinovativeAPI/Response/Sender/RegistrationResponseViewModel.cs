using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.Sender
{
    public class RegistrationResponseViewModel
    {
        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _SenderID;

        public string SenderId
        {
            get => _SenderID.TrimIfNotNull();
            set => _SenderID = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _OTPRefID;

        public string OTPRefID
        {
            get => _OTPRefID.TrimIfNotNull();
            set => _OTPRefID = value.TrimIfNotNull();
        }

        public string SenderID
        {
            get => _SenderID.TrimIfNotNull();
            set => _SenderID = value.TrimIfNotNull();
        }

        [JsonProperty("IsOTPRequired")]
        public bool IsOTPRequired { get; set; }
    }
}