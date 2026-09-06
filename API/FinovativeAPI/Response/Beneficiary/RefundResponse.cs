using Project.Processing;

namespace Project.Response.Beneficiary
{
    public class RefundResponse
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private bool _IsOTP;

        public bool IsOTP
        {
            get => _IsOTP;
            set => _IsOTP = value;
        }
    }
}