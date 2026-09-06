using Project.Processing;

namespace Project.Response.DematSetting
{
    public class ViewModel
    {
        private int _DematSettingID;

        public int DematSettingID
        {
            get => _DematSettingID;
            set => _DematSettingID = value;
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

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _UserName;

        public string UserName
        {
            get => _UserName.TrimIfNotNull();
            set => _UserName = value.TrimIfNotNull();
        }

        private string _DisplayName;

        public string DisplayName
        {
            get => _DisplayName.TrimIfNotNull();
            set => _DisplayName = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private int _Sequence;

        public int Sequence
        {
            get => _Sequence;
            set => _Sequence = value;
        }

        private string _UTMSource;

        public string UTMSource
        {
            get => _UTMSource.TrimIfNotNull();
            set => _UTMSource = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        public string IsActiveStr
        {
            get => (IsActive) ? "Yes" : "No";
        }
    }
}