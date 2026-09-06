namespace Project.Response.CallbackMaster
{
    public class CashFreeCallBack
    {
        public Data data { get; set; }
        public string event_time { get; set; }
        public string type { get; set; }
    }

    public class Order
    {
        public string order_id { get; set; }
        
    }

    public class Upi
    {
       
        public string upi_id { get; set; }
    }

    public class Card
    {
        
        public string card_number { get; set; }
        public string card_network { get; set; }
        public string card_type { get; set; }
        public string card_sub_type { get; set; }
        public string card_country { get; set; }
        public string card_bank_name { get; set; }
    }

    public class PaymentMethod
    {
        public Upi upi { get; set; }
        public Card card { get; set; }
    }

    public class Payment
    {
        public string cf_payment_id { get; set; }
        public string payment_status { get; set; }
        public double payment_amount { get; set; }
        public string payment_currency { get; set; }
        public string payment_message { get; set; }
        public string payment_time { get; set; }
        public string bank_reference { get; set; }
        public string auth_id { get; set; }
        public PaymentMethod payment_method { get; set; }
        public string payment_group { get; set; }
    }

    public class CustomerDetails
    {
        public string customer_name { get; set; }
        public string customer_id { get; set; }
        public string customer_email { get; set; }
        public string customer_phone { get; set; }
    }

    public class Data
    {
        public Order order { get; set; }
        public Payment payment { get; set; }
        public CustomerDetails customer_details { get; set; }
    }
}