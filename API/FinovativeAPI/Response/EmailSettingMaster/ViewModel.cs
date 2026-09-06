using Project.Processing;

namespace Project.Response.EmailSettingMaster
{
    public class ViewModel
    {
        private int _EmailSettingMasterID;

        public int EmailSettingMasterID
        {
            get => _EmailSettingMasterID;
            set => _EmailSettingMasterID = value;
        }

        private string _Email;

        public string Email
        {
            get => _Email.TrimIfNotNull();
            set => _Email = value.TrimIfNotNull();
        }

        private string _Password;

        public string Password
        {
            get => _Password.TrimIfNotNull();
            set => _Password = value.TrimIfNotNull();
        }

        private string _HostName;

        public string HostName
        {
            get => _HostName.TrimIfNotNull();
            set => _HostName = value.TrimIfNotNull();
        }

        private int _Port;

        public int Port
        {
            get => _Port;
            set => _Port = value;
        }

        private bool _IsSSL;

        public bool IsSSL
        {
            get => _IsSSL;
            set => _IsSSL = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }
    }
}