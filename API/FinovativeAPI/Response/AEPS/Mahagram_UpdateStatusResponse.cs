using Project.Processing;
using System.Text.Json.Serialization;

namespace Project.Response.AEPS
{
    public class Mahagram_UpdateStatusResponse
    {
        private string _MESSAGE;

        [JsonPropertyName("MESSAGE")]
        public string Message
        {
            get => _MESSAGE.TrimIfNotNull();
            set => _MESSAGE = value.TrimIfNotNull();
        }

        private string _STATUS;

        [JsonPropertyName("STATUS")]
        public string Status
        {
            get => _STATUS.TrimIfNotNull();
            set => _STATUS = value.TrimIfNotNull();
        }
    }
}