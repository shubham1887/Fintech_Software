using Project.Processing;

namespace Project.Response.DMTSettings
{
    public class ViewModel
    {
        private int _DMTSettingID;

        public int DMTSettingID
        {
            get => _DMTSettingID;
            set => _DMTSettingID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();

            set => _Name = value.TrimIfNotNull();
        }

        private bool _IsActiveForWebsite;

        public bool IsActiveForWebsite
        {
            get => _IsActiveForWebsite;
            set => _IsActiveForWebsite = value;
        }

        private string _DisplayName;

        public string DisplayName
        {
            get => _DisplayName.TrimIfNotNull();
            set => _DisplayName = value.TrimIfNotNull();
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

        private string _WalletID;

        public string WalletID
        {
            get => _WalletID.TrimIfNotNull();
            set => _WalletID = value.TrimIfNotNull();
        }

        private bool _IsPayoutDMT;

        public bool IsPayoutDMT
        {
            get => _IsPayoutDMT;
            set => _IsPayoutDMT = value;
        }

        public string IsPayoutDMTStr
        {
            get => (IsPayoutDMT) ? "Yes" : "No";
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

        private bool _IsCCF;

        public bool IsCCF
        {
            get => _IsCCF;
            set => _IsCCF = value;
        }

        private bool _IsNeft;

        public bool IsNeft
        {
            get => _IsNeft;
            set => _IsNeft = value;
        }

        private decimal _MinAmount;

        public decimal MinAmount
        {
            get => _MinAmount;
            set => _MinAmount = value;
        }

        private decimal _SplitAmount;

        public decimal SplitAmount
        {
            get => _SplitAmount;
            set => _SplitAmount = value;
        }

        private decimal _SenderLimit;

        public decimal SenderLimit
        {
            get => _SenderLimit;
            set => _SenderLimit = value;
        }

        private int _Sequence;

        public int Sequence
        {
            get => _Sequence;
            set => _Sequence = value;
        }

        public string IsActiveStr
        {
            get => (IsActive) ? "Yes" : "No";
        }

        public string IsCCFStr
        {
            get => (IsCCF) ? "Yes" : "No";
        }

        public string IsNeftStr
        {
            get => (IsNeft) ? "Yes" : "No";
        }

        private string _Mode;

        public string Mode
        {
            get => _Mode.TrimIfNotNull();
            set => _Mode = value.TrimIfNotNull();
        }

        private bool _IsExpressDMT;
        public bool IsExpressDMT
        {
            get => _IsExpressDMT;
            set => _IsExpressDMT = value;
        }
        public string IsExpressDMTStr
        {
            get => (IsExpressDMT) ? "Yes" : "No";
        }
    }
}