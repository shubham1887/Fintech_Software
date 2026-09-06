using Newtonsoft.Json;

namespace Project.Response.MicroATM
{
    public class MosambeeCallbackResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("businessName")]
        public string BusinessName { get; set; }

        [JsonProperty("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("addressLine2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("transactionDate")]
        public string TransactionDate { get; set; }

        [JsonProperty("transactionTime")]
        public string TransactionTime { get; set; }

        [JsonProperty("transactionLat")]
        public string TransactionLat { get; set; }

        [JsonProperty("transactionLong")]
        public string TransactionLong { get; set; }

        [JsonProperty("transactionSTAN")]
        public string TransactionStan { get; set; }

        [JsonProperty("transactionRRN")]
        public string TransactionRrn { get; set; }

        [JsonProperty("transactionAmount")]
        public string TransactionAmount { get; set; }

        [JsonProperty("transactionAuthCode")]
        public string TransactionAuthCode { get; set; }

        [JsonProperty("transactionBatchNumber")]
        public string TransactionBatchNumber { get; set; }

        [JsonProperty("transactionCardNumber")]
        public string TransactionCardNumber { get; set; }

        [JsonProperty("transactionTerminalId")]
        public string TransactionTerminalId { get; set; }

        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; }

        [JsonProperty("acquirerName")]
        public string AcquirerName { get; set; }

        [JsonProperty("customerSignature")]
        public string CustomerSignature { get; set; }

        [JsonProperty("transactionID")]
        public string TransactionId { get; set; }

        [JsonProperty("transactionTypeId")]
        public string TransactionTypeId { get; set; }

        [JsonProperty("billNumber")]
        public string BillNumber { get; set; }

        [JsonProperty("cardType")]
        public string CardType { get; set; }

        [JsonProperty("currencyId")]
        public string CurrencyId { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("narration")]
        public string Narration { get; set; }

        [JsonProperty("cardHolderName")]
        public string CardHolderName { get; set; }

        [JsonProperty("transactionTypeName")]
        public string TransactionTypeName { get; set; }

        [JsonProperty("aid")]
        public string Aid { get; set; }

        [JsonProperty("ici")]
        public string Ici { get; set; }

        [JsonProperty("apn")]
        public string Apn { get; set; }

        [JsonProperty("appLabel")]
        public string AppLabel { get; set; }

        [JsonProperty("tvr")]
        public string Tvr { get; set; }

        [JsonProperty("tsi")]
        public string Tsi { get; set; }

        [JsonProperty("ac")]
        public string Ac { get; set; }

        [JsonProperty("cid")]
        public string Cid { get; set; }

        [JsonProperty("tipProcessing")]
        public string TipProcessing { get; set; }

        [JsonProperty("transactionMode")]
        public string TransactionMode { get; set; }

        [JsonProperty("appVersion")]
        public string AppVersion { get; set; }

        [JsonProperty("MsrAndPinVerification")]
        public string MsrAndPinVerification { get; set; }

        [JsonProperty("cvm")]
        public string Cvm { get; set; }

        [JsonProperty("refTxnId")]
        public string RefTxnId { get; set; }

        [JsonProperty("cashBack")]
        public string CashBack { get; set; }

        [JsonProperty("tipAmount")]
        public string TipAmount { get; set; }

        [JsonProperty("creditDebitCardType")]
        public string CreditDebitCardType { get; set; }

        [JsonProperty("transactionStatus")]
        public string TransactionStatus { get; set; }

        [JsonProperty("checksum")]
        public string Checksum { get; set; }
    }
}