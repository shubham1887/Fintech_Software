namespace Project.Requests.PaymentGateway
{
    public class PaytmStatucCheckRequest
    {
        public string CHECKSUMHASH { get; internal set; }
        public string MID { get; internal set; }
        public string ORDERID { get; internal set; }
    }
}