using Project.Processing;

namespace Project.Response.PaymentGateway
{
    public class ViewModel
    {
        private int _PaymentGatewaySettingID;

        public int PaymentGatewaySettingID
        {
            get => _PaymentGatewaySettingID;
            set => _PaymentGatewaySettingID = value;
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

        private string _SuccessURL;

        public string SuccessURL
        {
            get => _SuccessURL.TrimIfNotNull();
            set => _SuccessURL = value.TrimIfNotNull();
        }

        private string _FailURL;

        public string FailURL
        {
            get => _FailURL.TrimIfNotNull();
            set => _FailURL = value.TrimIfNotNull();
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

        private string _ICICIQR;

        public string ICICIQR
        {
            get => _ICICIQR.TrimIfNotNull();
            set => _ICICIQR = value.TrimIfNotNull();
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private bool _IsUPI;

        public bool IsUPI
        {
            get => _IsUPI;
            set => _IsUPI = value;
        }

        public string IsActiveStr
        {
            get => (IsActive) ? "Yes" : "No";
        }

        public string IsUPIStr
        {
            get => (IsUPI) ? "Yes" : "No";
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
        private bool _IsManualStatusChange;

        public bool IsManualStatusChange
        {
            get => _IsManualStatusChange;
            set => _IsManualStatusChange = value;
        }

        private int _SettlementHoldInMinutes;
        public int SettlementHoldInMinutes
        {
            get => _SettlementHoldInMinutes;
            set => _SettlementHoldInMinutes = value;
        }
    }
}