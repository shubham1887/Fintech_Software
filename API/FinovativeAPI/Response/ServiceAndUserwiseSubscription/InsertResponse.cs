using Project.Processing;
using Project.Response.ServiceSubscription;
using System.Collections.Generic;

namespace Project.Response.ServiceAndUserwiseSubscription
{
    public class InsertResponse
    {
        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }

        private decimal _TotalAmount;

        public decimal TotalAmount
        {
            get => _TotalAmount;
            set => _TotalAmount = value;
        }

        private decimal _GST;

        public decimal GST
        {
            get => _GST;
            set => _GST = value;
        }

        private string _PlanName;

        public string PlanName
        {
            get => _PlanName.TrimIfNotNull();
            set => _PlanName = value.TrimIfNotNull();
        }

        private List<SubscriptionDetailResponse> _list;

        public List<SubscriptionDetailResponse> Services
        {
            get => _list;
            set => _list = value;
        }
    }
}
