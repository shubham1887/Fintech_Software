using Project.Processing;

namespace Project.Requests.ReferralRegistration
{
    public class SelectReferralRequest
    {
        private int _TakeCount;

        public int TakeCount
        {
            get => _TakeCount;
            set => _TakeCount = value;
        }

        private int _SkipCount;

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
        }

        private bool _IsExcelDownload;
        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
    }
}
