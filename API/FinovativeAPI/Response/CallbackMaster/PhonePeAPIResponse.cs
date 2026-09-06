using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.CallbackMaster
{
    public class PhonePeAPIResponse
    {
        private string _response;

        [JsonProperty("response")]
        public string response
        {
            get => _response.TrimIfNotNull();
            set => _response = value.TrimIfNotNull();
        }
    }
}