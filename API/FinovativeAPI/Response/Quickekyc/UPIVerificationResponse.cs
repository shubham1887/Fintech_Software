using Project.Processing;

namespace Project.Response.Quickekyc
{
    public class UPIVerificationResponse
    {
        private string _full_name;

        public string full_name
        {
            get => _full_name.TrimIfNotNull();
            set => _full_name = value.TrimIfNotNull();
        }

        private int _request_id;

        public int request_id
        {
            get => _request_id;
            set => _request_id = value;
        }
        
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


        private bool _account_exists;

        public bool account_exists
        {
            get => _account_exists;
            set => _account_exists = value;
        }

        
    }
}