namespace Project.Requests.PaymentGateway
{
    public class CashFreeOrderRequest
    {
        public string appId { get; set; }
        public string order_id { get; set; }
        public string order_amount { get; set; }
        public string order_currency { get; set; }
        public string orderNote { get; set; }

        public CustomerDetails customer_details { get; set; }
        public orderMeta order_meta { get; set; }

        public CashFreeOrderRequest()
        {
            customer_details=new CustomerDetails();
            order_meta = new orderMeta();
        }

    }
    public class CustomerDetails
    {
        public string customer_id { get; set; }
        public string customer_email { get; set; }
        public string customer_phone { get; set; }

    }
    public class orderMeta
    {
        public string return_url { get; set; }
        public string notify_url { get; set; }


    }

}