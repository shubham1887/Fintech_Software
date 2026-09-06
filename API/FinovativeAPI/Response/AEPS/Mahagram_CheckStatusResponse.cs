using Project.Processing;
using System.Text.Json.Serialization;

namespace Project.Response.AEPS
{
    public class Mahagram_CheckStatusResponse
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

        private string _TRANSACTION_ID;

        [JsonPropertyName("TRANSACTION_ID")]
        public string TransactionID
        {
            get => _TRANSACTION_ID.TrimIfNotNull();
            set => _TRANSACTION_ID = value.TrimIfNotNull();
        }

        private string _VENDOR_ID;

        [JsonPropertyName("VENDOR_ID")]
        public string VendorID
        {
            get => _VENDOR_ID.TrimIfNotNull();
            set => _VENDOR_ID = value.TrimIfNotNull();
        }
    }
}