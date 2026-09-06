using Newtonsoft.Json;

namespace YesBankCore.Response
{
    internal class PayoutAPIResponse
    {
        [JsonProperty("Data")]
        public DataResponse Data { get; set; }

        [JsonProperty("Risk")]
        public RiskResponse Risk { get; set; }

        

        [JsonProperty("ActionDescription")]
        public string ActionDescription { get; set; }

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("Meta")]
        public MetaResponse Meta { get; set; }
    }

    internal class DataResponse
    {
        [JsonProperty("ConsentId")]
        public string ConsentId { get; set; }

        [JsonProperty("TransactionIdentification")]
        public string TransactionIdentification { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        

        [JsonProperty("Initiation")]
        public InitiationResponse Initiation { get; set; }
    }

    internal class InitiationResponse
    {
        [JsonProperty("InstructionIdentification")]
        public string OrderID { get; set; }

        [JsonProperty("EndToEndIdentification")]
        public string BankRRN { get; set; }

        [JsonProperty("InstructedAmount")]
        public InstructedAmountResponse InstructedAmount { get; set; }

        [JsonProperty("DebtorAccount")]
        public DebtorAccountResponse DebtorAccount { get; set; }

        [JsonProperty("CreditorAccount")]
        public CreditorAccountResponse CreditorAccount { get; set; }

        
    }

    internal class CreditorAccountResponse
    {
        [JsonProperty("SchemeName")]
        public string SchemeName { get; set; }

        [JsonProperty("Identification")]
        public string Identification { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

         
    }
     

    internal class DebtorAccountResponse
    {
        [JsonProperty("Identification")]
        public string Identification { get; set; }

        [JsonProperty("SecondaryIdentification")]
        public string SecondaryIdentification { get; set; }

        

        
    }

    internal class InstructedAmountResponse
    {
        [JsonProperty("Amount")]
        public string Amount { get; set; }

        [JsonProperty("Currency")]
        public string Currency { get; set; }
    }

    

     

     

    internal class RiskResponse
    {
        [JsonProperty("PaymentContextCode")]
        public string PaymentContextCode { get; set; }

        
    }

    internal class MetaResponse
    {
        [JsonProperty("ErrorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("ErrorSeverity")]
        public string ErrorSeverity { get; set; }
    }

   
}