using Project.Processing;

namespace Project.Response.VerificationSetting
{
    public class ViewModel
    {
        private int _VerificationSettingID;

        public int VerificationSettingID
        {
            get => _VerificationSettingID;
            set => _VerificationSettingID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();

            set => _Name = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }

        private string _AuthCode;

        public string AuthCode
        {
            get => _AuthCode.TrimIfNotNull();
            set => _AuthCode = value.TrimIfNotNull();
        }

        private bool _IsAccountVerify;

        public bool IsAccountVerify
        {
            get => _IsAccountVerify;
            set => _IsAccountVerify = value;
        }

        private bool _IsUPIVerify;

        public bool IsUPIVerify
        {
            get => _IsUPIVerify;
            set => _IsUPIVerify = value;
        }

        private bool _IsPayoutVerify;

        public bool IsPayoutVerify
        {
            get => _IsPayoutVerify;
            set => _IsPayoutVerify = value;
        }   

        private string _CallBackURL;

        public string CallBackURL
        {
            get => _CallBackURL.TrimIfNotNull();
            set => _CallBackURL = value.TrimIfNotNull();
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private int _Sequence;

        public int Sequence
        {
            get => _Sequence;
            set => _Sequence = value;
        }

        private string _Mode;

        public string Mode
        {
            get => _Mode.TrimIfNotNull();
            set => _Mode = value.TrimIfNotNull();
        }

        public string IsActiveStr
        {
            get => (IsActive) ? "Yes" : "No";
        }

        public string IsUPIVerifyStr
        {
            get => (IsUPIVerify) ? "Yes" : "No";
        }

        public string IsAccountVerifyStr
        {
            get => (IsAccountVerify) ? "Yes" : "No";
        }

        public string IsPayoutVerifyStr
        {
            get => (IsPayoutVerify) ? "Yes" : "No";
        }
    }
}
