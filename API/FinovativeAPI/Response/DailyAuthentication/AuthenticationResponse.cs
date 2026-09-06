using Project.Processing;

namespace Project.Response.DailyAuthentication
{
    public class Authentication
    {
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
        private string _MerAuthTxnId;

        public string MerAuthTxnId
        {
            get => _MerAuthTxnId.TrimIfNotNull();
            set => _MerAuthTxnId = value.TrimIfNotNull();
        }
    }
}