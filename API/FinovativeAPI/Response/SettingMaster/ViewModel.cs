using Project.Processing;

namespace Project.Response.SettingMaster
{
    public class ViewModel
    {
        private int _SettingMasterID;

        public int SettingMasterID
        {
            get => _SettingMasterID;
            set => _SettingMasterID = value;
        }

        private bool _IsOn;

        public bool IsOn
        {
            get => _IsOn;
            set => _IsOn = value;
        }

        private bool _IsSecure;

        public bool IsSecure
        {
            get => _IsSecure;
            set => _IsSecure = value;
        }

        private bool _IsPassword;

        public bool IsPassword
        {
            get => _IsPassword;
            set => _IsPassword = value;
        }

        private string _KeyText;

        public string KeyText
        {
            get => _KeyText.TrimIfNotNull();
            set => _KeyText = value.TrimIfNotNull();
        }

        private string _ValueText;

        public string ValueText
        {
            get => _ValueText.TrimIfNotNull();
            set => _ValueText = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        public ValueDescription Value = new ValueDescription();
        public KeyIsPassword Key = new KeyIsPassword();
    }

    public class KeyIsPassword
    {
        private string _KeyText;

        public string KeyText
        {
            get => _KeyText.TrimIfNotNull();
            set => _KeyText = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }
    }

    public class ValueDescription
    {
        private string _ValueText;

        public string ValueText
        {
            get => _ValueText.TrimIfNotNull();
            set => _ValueText = value.TrimIfNotNull();
        }

        private bool _IsPassword;

        public bool IsPassword
        {
            get => _IsPassword;
            set => _IsPassword = value;
        }
    }
}