using Project.Processing;

namespace Project.Response.ServiceAndUserwiseSubscription
{
    public class PaymentDueResponse
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

        private int _Days;

        public int Days
        {
            get => _Days;
            set => _Days = value;
        }

        private bool _IsPaymentOverDue;

        public bool IsPaymentOverDue
        {
            get => _IsPaymentOverDue;
            set => _IsPaymentOverDue = value;
        }
        
    }
}
