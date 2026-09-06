using Project.Processing;

namespace Project.Response.AutoTransferUserwiseMapping
{
    public class GenerateOTPResponse
    {
        public bool IsOTPSent;
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
    }
}