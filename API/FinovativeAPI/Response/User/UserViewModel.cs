using Project.Processing;
using Project.Response.FCMResponse;

namespace Project.Response.User
{
    public class UserViewModel
    {
        public UserViewModel()
        {
            _FcmViewModel = new FCMViewModel();
        }

        private int _UserID;
        public int UserID { get => _UserID; set => _UserID = value; }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _WhatsAppNumber;

        public string WhatsAppNumber
        {
            get => _WhatsAppNumber.TrimIfNotNull();
            set => _WhatsAppNumber = value.TrimIfNotNull();
        }

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        private FCMViewModel _FcmViewModel;

        public FCMViewModel FcmViewModel
        {
            get => _FcmViewModel;
            set => _FcmViewModel = value;
        }

        private int _ParentID;

        public int ParentID
        {
            get => _ParentID;
            set => _ParentID = value;
        }

        private string _UserType;

        public string UserType
        {
            get => _UserType.TrimIfNotNull();
            set => _UserType = value.TrimIfNotNull();
        }
    }
}