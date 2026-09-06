using Project.Processing;

namespace Project.Response.MicroATM
{
    public class InitiateResponse
    {
        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _UniqueRefNo;

        public string UniqueRefNo
        {
            get => _UniqueRefNo.TrimIfNotNull();
            set => _UniqueRefNo = value.TrimIfNotNull();
        }

        private string _RequestId;

        public string RequestId
        {
            get => _RequestId.TrimIfNotNull();
            set => _RequestId = value.TrimIfNotNull();
        }

        private string _UserMobileNumber;

        public string UserMobileNumber
        {
            get => _UserMobileNumber.TrimIfNotNull();
            set => _UserMobileNumber = value.TrimIfNotNull();
        }

        private string _CustomerNumber;

        public string CustomerNumber
        {
            get => _CustomerNumber.TrimIfNotNull();
            set => _CustomerNumber = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _MerchantID;

        public string MerchantID
        {
            get => _MerchantID.TrimIfNotNull();
            set => _MerchantID = value.TrimIfNotNull();
        }

        private string _Slat;

        public string Slat
        {
            get => _Slat.TrimIfNotNull();
            set => _Slat = value.TrimIfNotNull();
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }

        private string _PublicKey;

        public string PublicKey
        {
            get => _PublicKey.TrimIfNotNull();
            set => _PublicKey = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private string _Password;

        public string Password
        {
            get => _Password.TrimIfNotNull();
            set => _Password = value.TrimIfNotNull();
        }
    }
}