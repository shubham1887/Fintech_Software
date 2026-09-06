using Project.Processing;

namespace Project.Response.ProviderRequestResponseLogResponse
{
    public class RequestResponseViewModel
    {
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private string _Request;

        public string Request
        {
            get => _Request.TrimIfNotNull();
            set => _Request = value.TrimIfNotNull();
        }

        private string _Response;

        public string Response
        {
            get => _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }

        private int providerID;

        public int ProviderID
        {
            get => providerID;
            set => providerID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _CallbackResponse;

        public string CallbackResponse
        {
            get => _CallbackResponse.TrimIfNotNull();
            set => _CallbackResponse = value.TrimIfNotNull();
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _ProviderRequestID;

        public string ProviderRequestID
        {
            get => _ProviderRequestID.TrimIfNotNull();
            set => _ProviderRequestID = value.TrimIfNotNull();
        }

        private int _TransactionID;

        public int TransactionID
        {
            get => _TransactionID;
            set => _TransactionID = value;
        }

        private string _ProviderReply;

        public string ProviderReply
        {
            get => _ProviderReply.TrimIfNotNull();
            set => _ProviderReply = value.TrimIfNotNull();
        }
     
    }
}