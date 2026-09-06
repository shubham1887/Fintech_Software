using Project.Processing;

namespace Project.Response.PayoutSetting
{
    public class ViewModel
    {
        private int _PayoutSettingID;

        public int PayoutSettingID
        {
            get => _PayoutSettingID;
            set => _PayoutSettingID = value;
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

        public string Mode
        {
            get => _Mode.TrimIfNotNull();
            set => _Mode = value.TrimIfNotNull();
        }
    }
}