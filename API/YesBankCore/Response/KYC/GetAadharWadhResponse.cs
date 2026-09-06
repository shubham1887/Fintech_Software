using YESBankCore.Controller;

namespace YesBankCore.Response.KYC
{
    public class GetAadharWadhResponse
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

        private string _Wadh;

        public string Wadh
        {
            get => _Wadh.TrimIfNotNull();
            set => _Wadh = value.TrimIfNotNull();
        }
    }
}
