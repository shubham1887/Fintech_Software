using System.Collections.Generic;

namespace Project.Response.PaymentGateway
{
    public class RazorpayQRResponse
    {
        public string id { get; set; }
        public string entity { get; set; }
        public int created_at { get; set; }
        public object name { get; set; }
        public string usage { get; set; }
        public string type { get; set; }
        public string image_url { get; set; }
        public int payment_amount { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public bool fixed_amount { get; set; }
        public int payments_amount_received { get; set; }
        public int payments_count_received { get; set; }
        public List<object> notes { get; set; }
        public object customer_id { get; set; }
        public object close_by { get; set; }
        public List<object> tax_invoice { get; set; }
    }
}