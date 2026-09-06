using Project.Processing;

namespace Project.Response.User
{
    public class RoleWiseUserViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _ParentName;

        public string ParentName
        {
            get => _ParentName.ToTitleCase();
            set => _ParentName = value.ToTitleCase();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.ToTitleCase();
            set => _FirmName = value.ToTitleCase();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }
    }
}