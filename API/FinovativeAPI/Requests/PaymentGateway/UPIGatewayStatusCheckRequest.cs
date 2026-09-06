namespace Project.Requests.PaymentGateway
{
    public class UPIGatewayStatusCheckRequest
    {
        public string key { get; set; }

        public string client_txn_id { get; set; }

        public string txn_date { get; set; }
    }
}