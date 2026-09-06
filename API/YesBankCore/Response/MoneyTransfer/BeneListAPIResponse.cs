using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    internal class BeneListAPIResponse
    {
        private string _responseMsg;

        public string responseMsg
        {
            get => _responseMsg.TrimIfNotNull();
            set => _responseMsg = value.TrimIfNotNull();
        }

        public List<BeneListErrorResponse> errorResponse { get; set; }

        public BeneListData data { get; set; }

    }

    public class BeneListData
    {
        public List<BeneficiaryDetail> beneficiaryDetails { get; set; }
    }

    public class BeneficiaryDetail
    {
        private string _masterBeneficiaryId;

        public string masterBeneficiaryId
        {
            get => _masterBeneficiaryId.TrimIfNotNull();
            set => _masterBeneficiaryId = value.TrimIfNotNull();
        }

        private string _masterCustomerId;

        public string masterCustomerId
        {
            get => _masterCustomerId.TrimIfNotNull();
            set => _masterCustomerId = value.TrimIfNotNull();
        }

        private string _accountNo;

        public string accountNo
        {
            get => _accountNo.TrimIfNotNull();
            set => _accountNo = value.TrimIfNotNull();
        }

        private string _ifscCode;

        public string ifscCode
        {
            get => _ifscCode.TrimIfNotNull();
            set => _ifscCode = value.TrimIfNotNull();
        }

        private string _verifiedBenName;

        public string verifiedBenName
        {
            get => _verifiedBenName.TrimIfNotNull();
            set => _verifiedBenName = value.TrimIfNotNull();
        }

        private string _isVerifiedBenName;

        public string isVerifiedBenName
        {
            get => _isVerifiedBenName.TrimIfNotNull();
            set => _isVerifiedBenName = value.TrimIfNotNull();
        }

        private string _name;

        public string name
        {
            get => _name.TrimIfNotNull();
            set => _name = value.TrimIfNotNull();
        }

        private string _status;

        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }
    }

    public class BeneListErrorResponse
    {
        private string _errorMsg;

        public string errorMsg
        {
            get => _errorMsg.TrimIfNotNull();
            set => _errorMsg = value.TrimIfNotNull();
        }
    }
}
