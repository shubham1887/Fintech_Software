using Project.Processing;

namespace Project.Response.User
{
    public class MorePayUserOnBoardingResponse
    {
        private string _MorePayMerchantCode;

        public string MorePayMerchantCode
        {
            get => _MorePayMerchantCode.TrimIfNotNull();
            set => _MorePayMerchantCode = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }
    }
}