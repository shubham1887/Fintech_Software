using Project.Processing;

namespace Project.Requests.AEPS
{
    public class EzyPayGenerateSessionIDRequest
    {
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
    }
}