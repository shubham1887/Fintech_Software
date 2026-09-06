using System;

namespace Project.Requests.User
{
    public class SelectDatewiseRequest
    {
        private DateTime _FromDate;

        public DateTime FromDate
        {
            get => _FromDate;

            set => _FromDate = value;
        }

        private DateTime _ToDate;

        public DateTime ToDate
        {
            get => _ToDate;

            set => _ToDate = value;
        }

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

        public bool IsExcelDownload { get; set; }

        private string _Search;

        public string Search
        {
            get => _Search;
            set => _Search = value;
        }
    }
}