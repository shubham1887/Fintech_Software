using YESBankCore.Controller;

namespace YesBankCore.Request.BBPS
{
    public class EncryptedRequest
    {
        private string _request;
        public string request
        {
            get => _request.TrimIfNotNull();
            set => _request = value.TrimIfNotNull();
        }
    }
}
