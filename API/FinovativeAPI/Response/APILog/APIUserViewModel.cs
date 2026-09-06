using Project.Processing;

namespace Project.Response.APILog
{
    public class APIUserViewModel
    {
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _FirmName;
        public string FirmName { get => _FirmName.TrimIfNotNull(); set => _FirmName = value.TrimIfNotNull(); }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _Request;

        public string Request
        {
            get => _Request.TrimIfNotNull();
            set => _Request = value.TrimIfNotNull();
        }

        private string _Response;

        public string Response
        {
            get => _Response.TrimIfNotNull();
            set => _Response = value.TrimIfNotNull();
        }

        private int dOCInt;

        public int DOCInt
        {
            get => dOCInt;
            set => dOCInt = value;
        }

        private int userID;

        public int UserID
        {
            get => userID;
            set => userID = value;
        }

        private string _DOCType;

        public string DOCType
        {
            get => _DOCType.TrimIfNotNull();
            set => _DOCType = value.TrimIfNotNull();
        }
    }
}