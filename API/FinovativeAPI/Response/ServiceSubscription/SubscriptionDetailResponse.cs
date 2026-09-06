namespace Project.Response.ServiceSubscription
{
    public class SubscriptionDetailResponse
    {
        private int _ServiceSubscriptionCreatePlanMasterID;
        public int ServiceSubscriptionCreatePlanMasterID
        {
            get => _ServiceSubscriptionCreatePlanMasterID;
            set => _ServiceSubscriptionCreatePlanMasterID = value;
        }

        private int _ServiceSubscriptionPlanDetailsID;

        public int ServiceSubscriptionPlanDetailsID
        {
            get => _ServiceSubscriptionPlanDetailsID;
            set => _ServiceSubscriptionPlanDetailsID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName;
            set => _ServiceName = value;
        }
    }
}