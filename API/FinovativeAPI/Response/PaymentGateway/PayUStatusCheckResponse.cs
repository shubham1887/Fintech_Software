using Newtonsoft.Json;
using System.Collections.Generic;

namespace Project.Response.PaymentGateway
{
    public class PayUStatusCheckResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }

        [JsonProperty("transaction_details")]
        public Dictionary<string, TransactionDetail> TransactionDetails { get; set; }
    }
    public class TransactionDetail
    {
        [JsonProperty("mihpayid")]
        public string MihpayId { get; set; }

        [JsonProperty("request_id")]
        public string RequestId { get; set; }

        [JsonProperty("bank_ref_num")]
        public string BankRefNum { get; set; }

        [JsonProperty("amt")]
        public string Amount { get; set; }

        [JsonProperty("transaction_amount")]
        public decimal TransactionAmount { get; set; }

        [JsonProperty("txnid")]
        public string TxnId { get; set; }

        [JsonProperty("bankcode")]
        public string BankCode { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty("card_type")]
        public string CardType { get; set; }

        [JsonProperty("error_Message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("net_amount_debit")]
        public decimal NetAmountDebit { get; set; }

        [JsonProperty("disc")]
        public decimal Discount { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

    }

}
