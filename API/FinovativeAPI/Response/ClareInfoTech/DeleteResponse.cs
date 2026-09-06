using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.ClareInfoTech
{
    public class DeleteResponse
    {
        private string _Status;

        private string _Message;

        [JsonProperty("Status")]
        public string Status
        {
            get
            {
                return _Status.TrimIfNotNull();
            }
            set
            {
                _Status = value.TrimIfNotNull();
            }
        }

        [JsonProperty("Message")]
        public string Message
        {
            get
            {
                return _Message.TrimIfNotNull();
            }
            set
            {
                _Message = value.TrimIfNotNull();
            }
        }

        private int _ISOTP;

        [JsonProperty("ISOTP")]
        public int ISOTP
        {
            get
            {
                return _ISOTP;
            }
            set
            {
                _ISOTP = value;
            }
        }

        private string _OTPReference;

        [JsonProperty("OTPReference")]
        public string OTPReference
        {
            get
            {
                return _OTPReference;
            }
            set
            {
                _OTPReference = value;
            }
        }
    }
}