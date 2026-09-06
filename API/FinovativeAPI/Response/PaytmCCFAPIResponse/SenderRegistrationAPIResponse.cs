using Project.Processing;

namespace Project.Response.PaytmCCFAPIResponse
{
    public class SenderRegistrationAPIResponse
    {
        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _message;
        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }

        private string _customerMobile;
        public string customerMobile
        {
            get => _customerMobile.TrimIfNotNull();
            set => _customerMobile = value.TrimIfNotNull();
        }


        private string _txn_id;
        public string txn_id
        {
            get => _txn_id.TrimIfNotNull();
            set => _txn_id = value.TrimIfNotNull();
        }
     
    }
}