using Project.Processing;

namespace Project.Response.AEPS
{
    public class Mahagram_InitiateResponseForMobile
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _SaltKey;

        public string SaltKey
        {
            get => _SaltKey.TrimIfNotNull();
            set => _SaltKey = value.TrimIfNotNull();
        }

        private string _SecretKey;

        public string SecretKey
        {
            get => _SecretKey.TrimIfNotNull();
            set => _SecretKey = value.TrimIfNotNull();
        }

        private string _MahagramBCID;

        public string MahagramBCID
        {
            get => _MahagramBCID.TrimIfNotNull();
            set => _MahagramBCID = value.TrimIfNotNull();
        }

        private string _EmailId;

        public string EmailID
        {
            get => _EmailId.TrimIfNotNull();
            set => _EmailId = value.TrimIfNotNull();
        }

        private string _Phone;

        public string Phone
        {
            get => _Phone.TrimIfNotNull();
            set => _Phone = value.TrimIfNotNull();
        }

        private string _CPID;

        public string CPID
        {
            get => _CPID.TrimIfNotNull();
            set => _CPID = value.TrimIfNotNull();
        }
    }
}