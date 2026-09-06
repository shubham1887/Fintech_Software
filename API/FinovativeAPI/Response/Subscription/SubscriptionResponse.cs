using Project.Processing;
using System;

namespace Project.Response.Subscription
{
    public class SubscriptionResponse
    {
        public DateTime EndDate { get; set; }
        public int RemainingDays { get; set; }
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _Message;

        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        private string _SubscriptionStatus;

        public string SubscriptionStatus
        {
            get => _SubscriptionStatus.TrimIfNotNull();
            set => _SubscriptionStatus = value.TrimIfNotNull();
        }
    }
}