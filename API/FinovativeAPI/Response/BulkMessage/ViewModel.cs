using Project.Processing;

namespace Project.Response.BulkMessage
{
    public class ViewModel
    {
        public int BulkMessageID { get; set; }

        public int UserID { get; set; }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        public bool IsSMS { get; set; }
        public bool IsWhatsUp { get; set; }

        public bool IsEmail { get; set; }

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

        private string _EmailID;

        public string EmailID
        {
            get => _EmailID.TrimIfNotNull();
            set => _EmailID = value.TrimIfNotNull();
        }

        public string BySMS
        {
            get => IsSMS ? "Yes" : "No";
        }

        public string ByEmail
        {
            get => IsEmail ? "Yes" : "No";
        }

        public string ByWhatsApp
        {
            get => IsWhatsUp ? "Yes" : "No";
        }
    }
}