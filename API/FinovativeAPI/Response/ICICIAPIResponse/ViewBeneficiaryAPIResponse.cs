using Newtonsoft.Json;
using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.ICICIAPIResponse
{
    public class ViewBeneficiaryAPIResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
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