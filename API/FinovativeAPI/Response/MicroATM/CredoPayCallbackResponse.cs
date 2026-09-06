using Newtonsoft.Json;

namespace Project.Response.MicroATM
{
    public class CredoPayCallbackResponse
    {
        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty("rrn")]
        public string Rrn { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("merchant_id")]
        public string MerchantId { get; set; }

        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty("tid")]
        public string Tid { get; set; }

        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("card_payment_type")]
        public string CardPaymentType { get; set; }

        [JsonProperty("card_usage_type")]
        public string CardUsageType { get; set; }

        [JsonProperty("masked_pan")]
        public string MaskedPan { get; set; }

        [JsonProperty("network")]
        public string Network { get; set; }

        [JsonProperty("response_description")]
        public string ResponseDescription { get; set; }

        [JsonProperty("custom_field1")]
        public string CustomField1 { get; set; }

        [JsonProperty("CRN_U")]
        public string CrnU { get; set; }

        [JsonProperty("aggregator_id")]
        public string AggregatorId { get; set; }

        [JsonProperty("bank_name")]
        public string BankName { get; set; }

        [JsonProperty("account_balance")]
        public string account_balance { get; set; }

        [JsonProperty("mini_statement")]
        public string mini_statement { get; set; }
    }
}