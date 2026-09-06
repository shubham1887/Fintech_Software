using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    public class BeneListResponse
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

        public List<BeneficiaryDetail> beneficiaryDetails { get; set; }
    }
}
