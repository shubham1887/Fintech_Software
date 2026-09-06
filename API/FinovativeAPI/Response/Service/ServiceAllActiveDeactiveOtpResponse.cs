using Project.Processing;

namespace Project.Response.Service
{
    public class ServiceAllActiveDeactiveOtpResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
        public bool IsOTPSent { get; set; }
    }
}
