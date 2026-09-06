using Newtonsoft.Json;

namespace Project.Response.PaymentGateway
{
    public class RazorPayWebHookResponse
    {
        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("contains")]
        public string[] Contains { get; set; }

        [JsonProperty("payload")]
        public Payload Payload { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }
    }

    public class Payload
    {
        [JsonProperty("payment")]
        public Payment Payment { get; set; }

        [JsonProperty("payment.downtime")]
        public PaymentDowntime PaymentDowntime { get; set; }
    }

    public class PaymentDowntime
    {
        [JsonProperty("entity")]
        public Entity Entity { get; set; }
    }

    public class Payment
    {
        [JsonProperty("entity")]
        public Entity Entity { get; set; }
    }

    public class Entity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("entity")]
        public string EntityEntity { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("invoice_id")]
        public string InvoiceId { get; set; }

        [JsonProperty("international")]
        public string International { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("amount_refunded")]
        public string AmountRefunded { get; set; }

        [JsonProperty("refund_status")]
        public string RefundStatus { get; set; }

        [JsonProperty("captured")]
        public bool Captured { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("card_id")]
        public string CardId { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        [JsonProperty("wallet")]
        public string Wallet { get; set; }

        [JsonProperty("vpa")]
        public string Vpa { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("contact")]
        public string Contact { get; set; }

        [JsonProperty("notes")]
        public string[] Notes { get; set; }

        [JsonProperty("fee")]
        public string Fee { get; set; }

        [JsonProperty("tax")]
        public string Tax { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

        [JsonProperty("error_source")]
        public string ErrorSource { get; set; }

        [JsonProperty("error_step")]
        public string ErrorStep { get; set; }

        [JsonProperty("error_reason")]
        public string ErrorReason { get; set; }

        [JsonProperty("acquirer_data")]
        public Hook_AcquirerData AcquirerData { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }

    public class Hook_AcquirerData
    {
        [JsonProperty("rrn")]
        public string Rrn { get; set; }
    }
}