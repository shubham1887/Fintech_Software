using Project.Processing;

namespace Project.Response.OTTSetting
{
    public class ViewModel
    {
        private int _OTTSettingID;

        public int OTTSettingID
        {
            get => _OTTSettingID;
            set => _OTTSettingID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _DisplayName;

        public string DisplayName
        {
            get => _DisplayName.TrimIfNotNull();
            set => _DisplayName = value.TrimIfNotNull();
        }

        private int _Sequence;

        public int Sequence
        {
            get => _Sequence;
            set => _Sequence = value;
        }

        private bool _IsActiveForWebsite;

        public bool IsActiveForWebsite
        {
            get => _IsActiveForWebsite;
            set => _IsActiveForWebsite = value;
        }

        private string _MerchantID;

        public string MerchantID
        {
            get => _MerchantID.TrimIfNotNull();
            set => _MerchantID = value.TrimIfNotNull();
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }

        private string _Slat;

        public string Slat
        {
            get => _Slat.TrimIfNotNull();
            set => _Slat = value.TrimIfNotNull();
        }

        private string _ActionURL;

        public string ActionURL
        {
            get => _ActionURL.TrimIfNotNull();
            set => _ActionURL = value.TrimIfNotNull();
        }

        private string _StatusCheckURL;

        public string StatusCheckURL
        {
            get => _StatusCheckURL.TrimIfNotNull();
            set => _StatusCheckURL = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        public string IsActiveStr
        {
            get => (IsActive) ? "Yes" : "No";
        }

        private decimal _MinimumAmount;

        public decimal MinimumAmount
        {
            get => _MinimumAmount;
            set => _MinimumAmount = value;
        }

        private decimal _MaximumAmount;

        public decimal MaximumAmount
        {
            get => _MaximumAmount;
            set => _MaximumAmount = value;
        }

        private string _Mode;
        internal int BusSettingMasterID;

        public string Mode
        {
            get => _Mode.TrimIfNotNull();
            set => _Mode = value.TrimIfNotNull();
        }
    }
}