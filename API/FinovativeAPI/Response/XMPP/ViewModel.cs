using Project.Processing;

namespace Project.Response.XMPP
{
    public class ViewModel
    {
        private string _HostName;

        public string HostName
        {
            get => _HostName.TrimIfNotNull();
            set => _HostName = value.TrimIfNotNull();
        }

        private string _WebUser;

        public string WebUser
        {
            get => _WebUser.TrimIfNotNull();
            set => _WebUser = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _OtomaxUser;

        public string OtomaxUser
        {
            get => _OtomaxUser.TrimIfNotNull();
            set => _OtomaxUser = value.TrimIfNotNull();
        }
    }
}