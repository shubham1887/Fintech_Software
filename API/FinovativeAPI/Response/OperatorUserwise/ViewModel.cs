using Project.Processing;

namespace Project.Response.OperatorUserwise
{
    public class ViewModel
    {
        private int _OperatorUserwiseID;

        public int OperatorUserwiseID
        {
            get => _OperatorUserwiseID;
            set => _OperatorUserwiseID = value;
        }

        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _OperatorName;

        public string OperatorName
        {
            get => _OperatorName.TrimIfNotNull();
            set => _OperatorName = value.TrimIfNotNull();
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _UsageLimit;

        public int UsageLimit
        {
            get => _UsageLimit;
            set => _UsageLimit = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private int userID;
        public int UserID { get => userID; set => userID = value; }

        private string userName;
        public string UserName { get => userName.TrimIfNotNull(); set => userName = value.TrimIfNotNull(); }
        private decimal _Todaysale;

        public decimal Todaysale
        {
            get => _Todaysale;
            set => _Todaysale = value;
        }

        private decimal _RemainingSale;

        public decimal RemainingSale
        {
            get => _RemainingSale;
            set => _RemainingSale = value;
        }

        private string _BlockedAmount;

        public string BlockedAmount
        {
            get => _BlockedAmount;
            set => _BlockedAmount = value;
        }
    }
}