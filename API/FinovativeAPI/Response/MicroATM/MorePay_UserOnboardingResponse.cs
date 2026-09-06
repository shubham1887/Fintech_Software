using Project.Processing;

namespace Project.Response.MicroATM
{
    public class MorePay_UserOnboardingResponse
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

        private string _MerchantCode;

        public string MerchantCode
        {
            get => _MerchantCode.TrimIfNotNull();
            set => _MerchantCode = value.TrimIfNotNull();
        }
    }
}