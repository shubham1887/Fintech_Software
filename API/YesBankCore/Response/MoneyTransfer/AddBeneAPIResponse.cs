using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    internal class AddBeneAPIResponse
    {

        private string _responseMsg;

        public string responseMsg
        {
            get => _responseMsg.TrimIfNotNull();
            set => _responseMsg = value.TrimIfNotNull();
        }

        public AddBeneData data { get; set; }

        public List<BeneAddErrorResponse> errorResponse { get; set; }

    }

    public class BeneAddErrorResponse
    {

        private string _errorMsg;

        public string errorMsg
        {
            get => _errorMsg.TrimIfNotNull();
            set => _errorMsg = value.TrimIfNotNull();
        }
    }

    public class AddBeneData
    {
        public Balance balance = new Balance();

        private string _masterBeneficiaryId;

        public string masterBeneficiaryId
        {
            get => _masterBeneficiaryId.TrimIfNotNull();
            set => _masterBeneficiaryId = value.TrimIfNotNull();
        }

    }

    public class Balance
    {
        private string _isActiveStatus;

        public string isActiveStatus
        {
            get => _isActiveStatus.TrimIfNotNull();
            set => _isActiveStatus = value.TrimIfNotNull();
        }
    }
}
