using Project.Processing;

namespace Project.Response.AEPS
{
    public class Paysprint_UserOnboardingResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Redirecturl;

        public string Redirecturl
        {
            get => _Redirecturl.TrimIfNotNull();
            set => _Redirecturl = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _PartnerID;

        public string PartnerID
        {
            get => _PartnerID.TrimIfNotNull();
            set => _PartnerID = value.TrimIfNotNull();
        }

        private string _JWTKey;

        public string JWTKey
        {
            get => _JWTKey.TrimIfNotNull();
            set => _JWTKey = value.TrimIfNotNull();
        }

        private string _MerchantCode;

        public string MerchantCode
        {
            get => _MerchantCode.TrimIfNotNull();
            set => _MerchantCode = value.TrimIfNotNull();
        }

        private string _MerchantMobile;

        public string MerchantMobile
        {
            get => _MerchantMobile.TrimIfNotNull();
            set => _MerchantMobile = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _Email;

        public string Email
        {
            get => _Email.TrimIfNotNull();
            set => _Email = value.TrimIfNotNull();
        }
    }
}