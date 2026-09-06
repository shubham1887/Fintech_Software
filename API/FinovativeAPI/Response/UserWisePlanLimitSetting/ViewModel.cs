using Project.Processing;

namespace Project.Response.UserWisePlanLimitSetting
{
    public class ViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
        private int _UserWisePlanLimitID;

        public int UserWisePlanLimitID
        {
            get => _UserWisePlanLimitID;
            set => _UserWisePlanLimitID = value;
        }
        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }
        private int _UserTypeID;

        public int UserTypeID
        {
            get => _UserTypeID;
            set => _UserTypeID = value;
        }

        private int _CircleHitCount;

        public int CircleHitCount
        {
            get => _CircleHitCount;
            set => _CircleHitCount = value;
        }

        private int _PlanHitCount;

        public int PlanHitCount
        {
            get => _PlanHitCount;
            set => _PlanHitCount = value;
        }
        
        private int _BillFetchHitCount;

        public int BillFetchHitCount
        {
            get => _BillFetchHitCount;
            set => _BillFetchHitCount = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }
    }
}
