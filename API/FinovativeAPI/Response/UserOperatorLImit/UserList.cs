using Project.Processing;

namespace Project.Response.UserOperatorLImit
{
    public class UserList
    {
        private int _UserID;
        public int UserID { get => _UserID; set => _UserID = value; }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }
    }
}