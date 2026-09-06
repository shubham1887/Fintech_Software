using Project.Processing;

namespace Project.Response.UserOnbording
{
    public class Fingpay_UserOnboardingResponse
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

        private string _Need2FA;

        public string Need2FA
        {
            get => _Need2FA.TrimIfNotNull();
            set => _Need2FA = value.TrimIfNotNull();
        }

        private string _MerchantId;

        public string MerchantId
        {
            get => _MerchantId.TrimIfNotNull();
            set => _MerchantId = value.TrimIfNotNull();
        }

        public int ActiveFlag { get; internal set; }
    }
}