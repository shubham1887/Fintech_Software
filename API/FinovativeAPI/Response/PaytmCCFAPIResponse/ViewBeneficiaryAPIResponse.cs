using Newtonsoft.Json;
using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.PaytmCCFAPIResponse
{
    public class ViewBeneficiaryAPIResponse
    {
        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        private string _response_code;
        public string response_code
        {
            get => _response_code.TrimIfNotNull();
            set => _response_code = value.TrimIfNotNull();
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
 

        private List<Response.Beneficiary.ViewModel> _List;

        [JsonProperty("List")]
        public List<Response.Beneficiary.ViewModel> List
        {
            get => _List;
            set => _List = value;
        }
    }


}
