using Project.Processing;

namespace Project.Response.PaysprintUPICollection
{
    public class StaticQRResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _StaticQRLink;

        public string StaticQRLink
        {
            get => _StaticQRLink.TrimIfNotNull();
            set => _StaticQRLink = value.TrimIfNotNull();
        }

        private string _RefID;

        public string RefID
        {
            get => _RefID.TrimIfNotNull();
            set => _RefID = value.TrimIfNotNull();
        }
    }
}