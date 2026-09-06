using Project.Processing;

namespace Project.Response.UPITransferSetting
{
    public class ViewModel
    {
        private int _UPITransferSettingID;

        public int UPITransferSettingID
        {
            get => _UPITransferSettingID;
            set => _UPITransferSettingID = value;
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

        private string _AuthCode;

        public string AuthCode
        {
            get => _AuthCode.TrimIfNotNull();
            set => _AuthCode = value.TrimIfNotNull();
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

        private decimal _MinAmount;

        public decimal MinAmount
        {
            get => _MinAmount;
            set => _MinAmount = value;
        }

        private decimal _MaximumAmount;

        public decimal MaximumAmount
        {
            get => _MaximumAmount;
            set => _MaximumAmount = value;
        }

        private decimal _SplitAmount;

        public decimal SplitAmount
        {
            get => _SplitAmount;
            set => _SplitAmount = value;
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
    }
}