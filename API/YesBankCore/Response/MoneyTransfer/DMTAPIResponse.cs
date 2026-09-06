using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    public class DMTAPIResponse
    {
        private string _eResp;

        public string eResp
        {
            get => _eResp.TrimIfNotNull();
            set => _eResp = value.TrimIfNotNull();
        }
    }
}
