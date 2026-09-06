using Newtonsoft.Json;

namespace Project.Response.CredoPay
{
    public class CredoPayLoginIDAndPassword
    {
        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}