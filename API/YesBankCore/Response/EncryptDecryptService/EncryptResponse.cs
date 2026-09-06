using YESBankCore.Controller;

namespace YesBankCore.Response.EncryptDecryptService
{
    public class EncryptResponse
    {
        private string _plainRequestIv;

        public string plainRequestIv
        {
            get => _plainRequestIv.TrimIfNotNull();
            set => _plainRequestIv = value.TrimIfNotNull();
        }
        private string _encryptedPartnerKey;

        public string encryptedPartnerKey
        {
            get => _encryptedPartnerKey.TrimIfNotNull();
            set => _encryptedPartnerKey = value.TrimIfNotNull();
        }
        private string _encryptedSecretKey;

        public string encryptedSecretKey
        {
            get => _encryptedSecretKey.TrimIfNotNull();
            set => _encryptedSecretKey = value.TrimIfNotNull();
        }
        private string _requestToken;

        public string requestToken
        {
            get => _requestToken.TrimIfNotNull();
            set => _requestToken = value.TrimIfNotNull();
        }
        private string _encryptRequestBody;

        public string encryptRequestBody
        {
            get => _encryptRequestBody.TrimIfNotNull();
            set => _encryptRequestBody = value.TrimIfNotNull();
        }
    }
}
