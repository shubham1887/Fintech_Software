namespace Project.Requests.PaymentGateway
{
    public class CashFreeOrderAndroidRequest
    {
        public string orderId { get; set; }
        public string orderAmount { get; set; }
        public string orderCurrency { get; set; }
    }
}