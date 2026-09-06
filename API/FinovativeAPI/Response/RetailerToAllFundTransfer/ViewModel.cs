using Project.Processing;

namespace Project.Response.RetailerToAllFundTransfer
{
    public class ViewModel
    {
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private bool _IsOtpRequired;

        public bool IsOtpRequired
        {
            get => _IsOtpRequired;
            set => _IsOtpRequired = value;
        }
    }
}