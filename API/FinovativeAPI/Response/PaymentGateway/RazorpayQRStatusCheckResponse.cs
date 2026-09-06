using System.Collections.Generic;

namespace Project.Response.PaymentGateway
{
    public class RazorpayQRStatusCheckResponse
    {
        public string entity { get; set; }
        public int count { get; set; }
        public List<Item> items { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public string entity { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }
        public string status { get; set; }
        public object order_id { get; set; }
        public object invoice_id { get; set; }
        public bool international { get; set; }
        public string method { get; set; }
        public int amount_refunded { get; set; }
        public object refund_status { get; set; }
        public bool captured { get; set; }
        public string description { get; set; }
        public object card_id { get; set; }
        public object bank { get; set; }
        public object wallet { get; set; }
        public string vpa { get; set; }
        public object email { get; set; }
        public object contact { get; set; }
        public List<object> notes { get; set; }
        public int fee { get; set; }
        public int tax { get; set; }
        public object error_code { get; set; }
        public string error_description { get; set; }
        public object error_source { get; set; }
        public object error_step { get; set; }
        public object error_reason { get; set; }

        public DataItems acquirer_data { get; set; }
        public int created_at { get; set; }
    }

    public class DataItems
    {
        public string rrn { get; set; }
    }
}