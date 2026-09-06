using Project.Processing;

namespace Project.Response.eKYC
{
    public class EkycDataViewModel
    {
        private int _UserID;
        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _FirmName;
        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _FullName;
        public string FullName
        {
            get => _FullName.TrimIfNotNull();
            set => _FullName = value.TrimIfNotNull();
        }
        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }
        private string _MobileNumber;

        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }
        private string _Gender;

        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }
        private string _PANNo;
        public string PANNo
        {
            get => _PANNo.TrimIfNotNull();
            set => _PANNo = value.TrimIfNotNull();
        }
        private string _DocumentType;

        public string DocumentType
        {
            get => _DocumentType.TrimIfNotNull();
            set => _DocumentType = value.TrimIfNotNull();
        }
        private string _DocumentNo;

        public string DocumentNo
        {
            get => _DocumentNo.TrimIfNotNull();
            set => _DocumentNo = value.TrimIfNotNull();
        }
        private string _Dob;

        public string Dob
        {
            get => _Dob.TrimIfNotNull();
            set => _Dob = value.TrimIfNotNull();
        }
        private string _Address;
        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _Pin;
        public string Pin
        {
            get => _Pin.TrimIfNotNull();
            set => _Pin = value.TrimIfNotNull();
        }

        private string profileImage;
        public string ProfileImage
        {
            get => profileImage.TrimIfNotNull();
            set => profileImage = value.TrimIfNotNull();
        }

    }
}
