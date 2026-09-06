using Project.Processing;

namespace Project.Response.UserOnbording
{
    public class UserOnbordingListResponse
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
            get => _FirmName.ToTitleCase();
            set => _FirmName = value.ToTitleCase();
        }

        private string _MerchantCode;

        public string MerchantCode
        {
            get => _MerchantCode.TrimIfNotNull();
            set => _MerchantCode = value.TrimIfNotNull();
        }
        private string _MobileNumber;
        public string MobileNumber
        {
            get => _MobileNumber.TrimIfNotNull();
            set => _MobileNumber = value.TrimIfNotNull();
        }
        private string _DOC;
        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }
    }
}