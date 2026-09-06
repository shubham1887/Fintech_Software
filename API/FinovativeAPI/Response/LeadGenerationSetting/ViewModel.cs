using Project.Processing;

namespace Project.Response.LeadGenerationSetting
{
    public class ViewModel
    {
        private int _LeadGenerationSettingID;

        public int LeadGenerationSettingID
        {
            get => _LeadGenerationSettingID;
            set => _LeadGenerationSettingID = value;
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
            get => _DisplayName.TrimIfNotNullWithDefault("LeadGeneration");
            set => _DisplayName = value.TrimIfNotNullWithDefault("LeadGeneration");
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

        private int _Sequence;

        public int Sequence
        {
            get => _Sequence;
            set => _Sequence = value;
        }

        private string _Slat;

        public string Slat
        {
            get => _Slat.TrimIfNotNull();
            set => _Slat = value.TrimIfNotNull();
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

        private string _PublicKey;

        public string PublicKey
        {
            get => _PublicKey.TrimIfNotNull();
            set => _PublicKey = value.TrimIfNotNull();
        }

        private string _Mode;

        public string Mode
        {
            get => _Mode.TrimIfNotNull();
            set => _Mode = value.TrimIfNotNull();
        }
    }
}