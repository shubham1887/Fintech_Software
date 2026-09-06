using YESBankCore.Controller;

namespace YesBankCore.Response.AEPS
{
    internal class AuthenticationAPIResponse
    {
        private string _responseCode;

        public string responseCode
        {
            get => _responseCode.TrimIfNotNull();
            set => _responseCode = value.TrimIfNotNull();
        }


        private string _responseMsg;

        public string responseMsg
        {
            get => _responseMsg.TrimIfNotNull();
            set => _responseMsg = value.TrimIfNotNull();
        }
        
        

        public AuthData data { get; set; }

        public List<ErrorResponse> errorResponse { get; set; }

    }

    public class AuthData
    {

        private string _sorTxnId;

        public string sorTxnId
        {
            get => _sorTxnId.TrimIfNotNull();
            set => _sorTxnId = value.TrimIfNotNull();
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
