using Project.Processing;

namespace Project.Requests.ServiceAndUserwiseSubscription
{
    public class SelectServiceChargeRequest
    {
        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

    }
}
