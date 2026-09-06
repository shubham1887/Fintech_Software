using Newtonsoft.Json;

namespace Project.Response.PaymentGateway
{
    public class RayzorpayWebHookQRResponse
    {
        [JsonProperty("entity")]
        public string entity { get; set; }

        [JsonProperty("account_id")]
        public string account_id { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("contains")]
        public string[] contains { get; set; }

        [JsonProperty("payload")]
        public PayloadQR payload { get; set; }

        [JsonProperty("created_at")]
        public int created_at { get; set; }
    }

    public class PayloadQR
    {
        [JsonProperty("payment")]
        public PaymentQR payment { get; set; }

        [JsonProperty("qr_code")]
        public QrCode qr_code { get; set; }
    }

    public class PaymentQR
    {
        [JsonProperty("entity")]
        public EntityQR entity { get; set; }
    }

    public class QrCode
    {
        [JsonProperty("entity")]
        public EntityQR entity { get; set; }
    }

    public class EntityQR
    {
        public string id { get; set; }
        public string entity { get; set; }
        public int amount { get; set; }
        public string currency { get; set; }
        public string status { get; set; }
        public string order_id { get; set; }
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
        public string email { get; set; }
        public string contact { get; set; }
        public string customer_id { get; set; }
        public Notes notes { get; set; }
        public int fee { get; set; }
        public int tax { get; set; }
        public object error_code { get; set; }
        public object error_description { get; set; }
        public object error_source { get; set; }
        public object error_step { get; set; }
        public object error_reason { get; set; }
        public ACQData acquirer_data { get; set; }
        public int created_at { get; set; }
        public string name { get; set; }
        public string usage { get; set; }
        public string type { get; set; }
        public string image_url { get; set; }
        public object payment_amount { get; set; }
        public bool fixed_amount { get; set; }
        public int payments_amount_received { get; set; }
        public int payments_count_received { get; set; }
        public int close_by { get; set; }
        public object closed_at { get; set; }
        public object close_reason { get; set; }
    }

    public class ACQData
    {
        public string rrn { get; set; }
    }

    public class Notes
    {
        public string Branch { get; set; }
    }
}