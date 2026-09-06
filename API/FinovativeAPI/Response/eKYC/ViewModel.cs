using Project.Processing;

namespace Project.Response.eKYC
{
    public class ViewModel
    {
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _DocumentNo;

        public string DocumentNo
        {
            get => _DocumentNo.TrimIfNotNull();
            set => _DocumentNo = value.TrimIfNotNull();
        }
        private string _EmailID;
        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }
        private string _PANNo;
        public string PANNo
        {
            get => _PANNo.TrimIfNotNull();
            set => _PANNo = value.TrimIfNotNull();
        }
        private string _MobileNumber;
        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }

        private string _Response;

        public string Response
        {
            get => _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }

        private string profileImage;

        public string ProfileImage
        {
            get => profileImage.TrimIfNotNull();
            set => profileImage = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
        private string _DocumentType;

        public string DocumentType
        {
            get => _DocumentType.TrimIfNotNull();
            set => _DocumentType = value.TrimIfNotNull();
        }
    }
}