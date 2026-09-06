using System.Collections.Generic;

namespace Project.Response.PaymentGateway
{
    public class RazorpayStatusCheckResponse
    {
        public string entity { get; set; }
        public int count { get; set; }
        public List<RazorpayStatusCheckItem> items { get; set; }
    }

    public class RazorpayStatusCheckItem
    {
        public string id { get; set; }
        public string entity { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }
        public string status { get; set; }
        public string order_id { get; set; }
        public string invoice_id { get; set; }
        public bool international { get; set; }
        public string method { get; set; }
        public int amount_refunded { get; set; }
        public string refund_status { get; set; }
        public bool captured { get; set; }
        public string description { get; set; }
        public string card_id { get; set; }
        public string bank { get; set; }
        public string wallet { get; set; }
        public string vpa { get; set; }
        public string email { get; set; }
        public string contact { get; set; }
        public List<object> notes { get; set; }
        public string fee { get; set; }
        public string tax { get; set; }
        public string error_code { get; set; }
        public string error_description { get; set; }
        public string error_source { get; set; }
        public string error_step { get; set; }
        public string error_reason { get; set; }
        public AcquirerData acquirer_data { get; set; }
        public int created_at { get; set; }
    }

    public class AcquirerData
    {
        public string rrn { get; set; }
        public string upi_transaction_id { get; set; }
        public string auth_code { get; set; }
        public string bank_transaction_id { get; set; }
    }
}