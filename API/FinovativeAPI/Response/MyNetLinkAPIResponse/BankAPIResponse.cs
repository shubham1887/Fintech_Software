using Newtonsoft.Json;
using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.MyNetLinkAPIResponse
{
    public class BankAPIResponse
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

        private List<Response.DMTBank.ViewModel> _List;

        [JsonProperty("List")]
        public List<Response.DMTBank.ViewModel> List
        {
            get => _List;
            set => _List = value;
        }
    }
}