using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.ServiceSubscription
{
    public class ViewModel
    {
        private int _ServiceSubscriptionCreatePlanMasterID;
        public int ServiceSubscriptionCreatePlanMasterID
        {
            get => _ServiceSubscriptionCreatePlanMasterID;
            set => _ServiceSubscriptionCreatePlanMasterID = value;
        }

        private string _PlanName;

        public string PlanName
        {
            get => _PlanName.TrimIfNotNull();
            set => _PlanName = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private string _ImageURL;

        public string ImageURL
        {
            get => _ImageURL.TrimIfNotNull();
            set => _ImageURL = value.TrimIfNotNull();
        }

        private bool _IsPlanPurchase;

        public bool IsPlanPurchase
        {
            get => _IsPlanPurchase;
            set => _IsPlanPurchase = value;
        }

        private List<SubscriptionDetailResponse> _list;

        public List<SubscriptionDetailResponse> Services
        {
            get => _list;
            set => _list = value;
        }
    }
}