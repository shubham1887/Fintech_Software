using Project.Processing;
using System;

namespace Project.Response.DematGrowthLossMaster
{
    public class ViewModel
    {
        private string _DOC;
        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }


        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }



        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }
        private decimal _Amount;
        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
        private string _GrowthLoss;
        public string GrowthLoss
        {
            get => _GrowthLoss.TrimIfNotNull();
            set => _GrowthLoss = value.TrimIfNotNull();
        }

        private string _Plan;

        public string Plan
        {
            get => _Plan.TrimIfNotNull();
            set => _Plan = value.TrimIfNotNull();
        }

        private int _DematGrowthLossMasterID;
        public int DematGrowthLossMasterID
        {
            get => _DematGrowthLossMasterID;
            set => _DematGrowthLossMasterID = value;
        }

        private int _UserID;
        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _ServiceSubscriptionPlanMasterID;
        public int ServiceSubscriptionPlanMasterID
        {
            get => _ServiceSubscriptionPlanMasterID;
            set => _ServiceSubscriptionPlanMasterID = value;
        }
        private int _DOCInt;
        public int DOCInt
        {
            get => _DOCInt;
            set => _DOCInt = value;
        }



        private int _APIProviderId;
        public int APIProviderId
        {
            get => _APIProviderId;
            set => _APIProviderId = value;
        }

        private int _ServiceID;
        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private DateTime _ManualDate;

        public DateTime ManualDate
        {
            get => _ManualDate;
            set => _ManualDate = value;
        }

    }
}
