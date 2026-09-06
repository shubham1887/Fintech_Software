using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.MicroATM
{
    public class CredoPayStatusCheckResponse
    {
        private string _Code;

        [JsonProperty("code")]
        public string Code
        {
            get => _Code.TrimIfNotNull();
            set => _Code = value.TrimIfNotNull();
        }

        private string _Message;

        [JsonProperty("message")]
        public string Message
        {
            get => _Message.TrimIfNotNull();
            set => _Message = value.TrimIfNotNull();
        }

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("curreny")]
        public string Curreny { get; set; }

        [JsonProperty("is_void")]
        public bool IsVoid { get; set; }

        [JsonProperty("is_reversed")]
        public string IsReversed { get; set; }

        [JsonProperty("is_refund_submitted")]
        public string IsRefundSubmitted { get; set; }

        [JsonProperty("is_disputed")]
        public string IsDisputed { get; set; }

        [JsonProperty("is_pin_entered")]
        public string IsPinEntered { get; set; }

        [JsonProperty("is_card_fallback")]
        public string IsCardFallback { get; set; }

        [JsonProperty("is_settled")]
        public string IsSettled { get; set; }

        [JsonProperty("is_hold")]
        public string IsHold { get; set; }

        [JsonProperty("CRN_U")]
        public string CrnU { get; set; }

        [JsonProperty("aid")]
        public string Aid { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("app_version")]
        public string AppVersion { get; set; }

        [JsonProperty("card_application_name")]
        public string CardApplicationName { get; set; }

        [JsonProperty("card_holder_name")]
        public string CardHolderName { get; set; }

        [JsonProperty("card_type")]
        public string CardType { get; set; }

        [JsonProperty("custom_field1")]
        public string CustomField1 { get; set; }

        [JsonProperty("masked_pan")]
        public string MaskedPan { get; set; }

        [JsonProperty("money_add_source")]
        public string MoneyAddSource { get; set; }

        [JsonProperty("network")]
        public string Network { get; set; }

        [JsonProperty("other_amount")]
        public string OtherAmount { get; set; }

        [JsonProperty("payment_method")]
        public string PaymentMethod { get; set; }

        [JsonProperty("tid")]
        public string Tid { get; set; }

        [JsonProperty("transaction_group")]
        public string TransactionGroup { get; set; }

        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty("tsi")]
        public string Tsi { get; set; }

        [JsonProperty("tvr")]
        public string Tvr { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("merchant_id")]
        public string MerchantId { get; set; }

        [JsonProperty("aggregator_id")]
        public string AggregatorId { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("rrn")]
        public string Rrn { get; set; }

        [JsonProperty("frm_checked")]
        public string FrmChecked { get; set; }

        [JsonProperty("frm_code")]
        public string FrmCode { get; set; }

        [JsonProperty("auth_code")]
        public string AuthCode { get; set; }

        [JsonProperty("card_payment_type")]
        public string CardPaymentType { get; set; }

        [JsonProperty("card_usage_type")]
        public string CardUsageType { get; set; }

        [JsonProperty("response_description")]
        public string ResponseDescription { get; set; }

        [JsonProperty("transaction_certificate")]
        public string TransactionCertificate { get; set; }

        [JsonProperty("id")]
        public string CredoPayStatusCheckResponseId { get; set; }
    }
}