using Project.Processing;

namespace Project.Response.AEPS
{
    public class EzyPayInitResponse
    {
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _CompanyName;

        public string CompanyName
        {
            get => _CompanyName.TrimIfNotNull();
            set => _CompanyName = value.TrimIfNotNull();
        }

        private string _Logo;

        public string Logo
        {
            get => _Logo.TrimIfNotNull();
            set => _Logo = value.TrimIfNotNull();
        }

        private string _RetailerID;

        public string RetailerID
        {
            get => _RetailerID.TrimIfNotNull();
            set => _RetailerID = value.TrimIfNotNull();
        }

        private string _MerchantID;

        public string MerchantID
        {
            get => _MerchantID.TrimIfNotNull();
            set => _MerchantID = value.TrimIfNotNull();
        }

        private string _AuthCode;

        public string AuthCode
        {
            get => _AuthCode.TrimIfNotNull();
            set => _AuthCode = value.TrimIfNotNull();
        }

        private string _Mpin;

        public string Mpin
        {
            get => _Mpin.TrimIfNotNull();
            set => _Mpin = value.TrimIfNotNull();
        }
    }
}