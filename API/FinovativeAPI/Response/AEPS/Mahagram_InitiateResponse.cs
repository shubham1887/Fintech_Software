using Project.Processing;

namespace Project.Response.AEPS
{
    public class Mahagram_InitiateResponse
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

        private string _VENDOR_ID;

        public string VENDOR_ID
        {
            get => _VENDOR_ID.TrimIfNotNull();
            set => _VENDOR_ID = value.TrimIfNotNull();
        }
    }
}