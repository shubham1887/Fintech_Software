using Project.Processing;

namespace Project.Response.PaymentGateway
{
    public class StatusCheckResponse
    {
        private string _TransactionID;

        public string TransactionID
        {
            get => _TransactionID.TrimIfNotNull();
            set => _TransactionID = value.TrimIfNotNull();
        }

        private string _Paymode;

        public string Paymode
        {
            get => _Paymode.TrimIfNotNull();
            set => _Paymode = value.TrimIfNotNull();
        }

        private string _OrderID;

        public string OrderID
        {
            get => _OrderID.TrimIfNotNull();
            set => _OrderID = value.TrimIfNotNull();
        }

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

        private string _Sender;

        public string Sender
        {
            get => _Sender.TrimIfNotNull();
            set => _Sender = value.TrimIfNotNull();
        }

        public decimal Amount { get; set; }
    }
}