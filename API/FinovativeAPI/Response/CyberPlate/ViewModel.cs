using Project.Processing;

namespace Project.Response.CyberPlate
{
    public class ViewModel
    {
        private int _CyberPlateSettingID;

        public int CyberPlateSettingID
        {
            get => _CyberPlateSettingID;
            set => _CyberPlateSettingID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _AP;

        public string AP
        {
            get => _AP.TrimIfNotNull();
            set => _AP = value.TrimIfNotNull();
        }

        private string _OP;

        public string OP
        {
            get => _OP.TrimIfNotNull();
            set => _OP = value.TrimIfNotNull();
        }

        private string _SD;

        public string SD
        {
            get => _SD.TrimIfNotNull();
            set => _SD = value.TrimIfNotNull();
        }

        private string _Password;

        public string Password
        {
            get => _Password.TrimIfNotNull();
            set => _Password = value.TrimIfNotNull();
        }

        private string _TermID;

        public string TermID
        {
            get => _TermID.TrimIfNotNull();
            set => _TermID = value.TrimIfNotNull();
        }

        private string _AID;

        public string AID
        {
            get => _AID.TrimIfNotNull();
            set => _AID = value.TrimIfNotNull();
        }

        private string _SecretKeyPath;

        public string SecretKeyPath
        {
            get => _SecretKeyPath.TrimIfNotNull();
            set => _SecretKeyPath = value.TrimIfNotNull();
        }

        private bool _IsActiveWeb;

        public bool IsActiveWeb
        {
            get => _IsActiveWeb;
            set => _IsActiveWeb = value;
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
    }
}