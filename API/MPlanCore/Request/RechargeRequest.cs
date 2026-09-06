using MplanCore.Controller;

namespace MplanCore.Request
{
    public class RechargeRequest
    {
        private string _api_token;

        public string api_token
        {
            get => _api_token.TrimIfNotNull();
            set => _api_token = value.TrimIfNotNull();
        }

        private string _user_id;

        public string user_id
        {
            get => _user_id.TrimIfNotNull();
            set => _user_id = value.TrimIfNotNull();
        }

        private string _opid;

        public string opid
        {
            get => _opid.TrimIfNotNull();
            set => _opid = value.TrimIfNotNull();
        }

        private string _circleid;

        public string circleid
        {
            get => _circleid.TrimIfNotNull();
            set => _circleid = value.TrimIfNotNull();
        }

        private string _mobile_number;

        public string mobile_number
        {
            get => _mobile_number.TrimIfNotNull();
            set => _mobile_number = value.TrimIfNotNull();
        }
    }
}