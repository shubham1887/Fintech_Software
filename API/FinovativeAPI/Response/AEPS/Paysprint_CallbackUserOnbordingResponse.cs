using Newtonsoft.Json;

namespace Project.Response.AEPS
{
    public class Paysprint_CallbackUserOnbordingResponse
    {
        [JsonProperty("event")]
        public string eventName { get; set; }

        public string param_inc { get; set; }

        public paramOnboarding param { get; set; }
    }

    public class paramOnboarding
    {
        public string request_id { get; set; }
        public string amount { get; set; }

        public string partner_id { get; set; }
        public string merchant_id { get; set; }
    }
}