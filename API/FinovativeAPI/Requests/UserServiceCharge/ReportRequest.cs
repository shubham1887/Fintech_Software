using System;

namespace Project.Requests.UserServiceCharge
{
    public class ReportRequest
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

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
    }
}