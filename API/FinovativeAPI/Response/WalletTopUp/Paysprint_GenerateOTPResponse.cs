using Project.Processing;

namespace Project.Response.WalletTopUp
{
    public class Paysprint_GenerateOTPResponse
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

        private string _RefernceNo;

        public string RefernceNo
        {
            get => _RefernceNo.TrimIfNotNull();
            set => _RefernceNo = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }
    }
}