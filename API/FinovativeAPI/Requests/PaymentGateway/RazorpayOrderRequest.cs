namespace Project.Requests.PaymentGateway
{
    public class RazorpayOrderRequest
    {
        public int amount { get; set; }
        public string currency { get; set; }
        public string receipt { get; set; }
    }
}