using Project.Processing;

namespace Project.Response.EcollectionSetting
{
    public class ViewModel
    {
        private int _EcollectionSettingID;

        public int EcollectionSettingID
        {
            get => _EcollectionSettingID;
            set => _EcollectionSettingID = value;
        }

        private string _PrivateKeyPath;

        public string PrivateKeyPath
        {
            get => _PrivateKeyPath.TrimIfNotNull();
            set => _PrivateKeyPath = value.TrimIfNotNull();
        }

        private string _PrivateKeyPassword;

        public string PrivateKeyPassword
        {
            get => _PrivateKeyPassword.TrimIfNotNull();
            set => _PrivateKeyPassword = value.TrimIfNotNull();
        }

        private string _PublicKeyPath;

        public string PublicKeyPath
        {
            get => _PublicKeyPath.TrimIfNotNull();
            set => _PublicKeyPath = value.TrimIfNotNull();
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

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private string _WalletName;

        public string WalletName
        {
            get => _WalletName.TrimIfNotNull();
            set => _WalletName = value.TrimIfNotNull();
        }

        private string _Prefix;

        public string Prefix
        {
            get => _Prefix.TrimIfNotNull();
            set => _Prefix = value.TrimIfNotNull();
        }

        private string _IFSCCode;

        public string IFSCCode
        {
            get => _IFSCCode.TrimIfNotNull();
            set => _IFSCCode = value.TrimIfNotNull();
        }
    }
}