using Project.Processing;

namespace Project.Response.User
{
    public class UserViewForBackup
    {
        private int _UserID;
        public int UserID { get => _UserID; set => _UserID = value; }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }
    }
}