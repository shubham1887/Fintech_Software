using YESBankCore.Controller;

namespace YesBankCore.Response.MoneyTransfer
{
    internal class SelectSenderAPIResponse
    {
        private string _responseMsg;

        public string responseMsg
        {
            get => _responseMsg.TrimIfNotNull();
            set => _responseMsg = value.TrimIfNotNull();
        }
        public SelectSenderData data { get; set; }

        public List<ErrorResponse> errorResponse { get; set; }
    }

    public class SelectSenderData
    {
        private string _name;

        public string name
        {
            get => _name.TrimIfNotNull();
            set => _name = value.TrimIfNotNull();
        }

        private string _custMiddleName;

        public string custMiddleName
        {
            get => _custMiddleName.TrimIfNotNull();
            set => _custMiddleName = value.TrimIfNotNull();
        }

        private string _custLastName;

        public string custLastName
        {
            get => _custLastName.TrimIfNotNull();
            set => _custLastName = value.TrimIfNotNull();
        }

        public List<Balance1> balance { get; set; }
    }

    public class Balance1
    {
        private string _monthlyAvailLimit;

        public string monthlyAvailLimit
        {
            get => _monthlyAvailLimit.TrimIfNotNull();
            set => _monthlyAvailLimit = value.TrimIfNotNull();
        }

        private string _monthlyAvailTxnCount;

        public string monthlyAvailTxnCount
        {
            get => _monthlyAvailTxnCount.TrimIfNotNull();
            set => _monthlyAvailTxnCount = value.TrimIfNotNull();
        }
    }


    public class ErrorResponse
    {
        private string _errorMsg;

        public string errorMsg
        {
            get => _errorMsg.TrimIfNotNull();
            set => _errorMsg = value.TrimIfNotNull();
        }
        
    }
}
