using Project.Processing;

namespace Project.Requests.User
{
    public class SelectUserForDropDownByUserIDRequest
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.ToTitleCase();
            set => _FirmName = value.ToTitleCase();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
    }
}