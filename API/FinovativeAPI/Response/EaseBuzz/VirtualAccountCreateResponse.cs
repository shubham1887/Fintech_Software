using Project.Processing;

namespace Project.Response.EaseBuzz
{
    public class VirtualAccountCreateResponse
    {
        private string _Status;
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

        private string _Message;
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

        private string _QRImage;
        public string QRImage
        {
            get
            {
                return _QRImage.TrimIfNotNull();
            }
            set
            {
                _QRImage = value.TrimIfNotNull();
            }
        }

    }
}