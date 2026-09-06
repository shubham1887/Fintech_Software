using Project.Processing;

namespace Project.Response.PaymentGateway
{
    public class SelectByIDResponse
    {
        private string _Status;

        public string Status { get => _Status.TrimIfNotNull(); set => _Status = value.TrimIfNotNull(); }

        private int _PaymentGatewayID;
        public int PaymentGatewayID { get => _PaymentGatewayID; set => _PaymentGatewayID = value; }
    }
}