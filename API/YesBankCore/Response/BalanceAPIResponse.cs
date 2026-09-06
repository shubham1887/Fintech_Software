using Newtonsoft.Json;

namespace YesBankCore.Response
{
    internal class BalanceAPIResponse
    {
        [JsonProperty("Data")]
        public BalanceDataResponse Data { get; set; }

        [JsonProperty("moreInformation")]
        public string MoreInformation { get; set; }

        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("ActionCode")]
        public string ActionCode { get; set; }

        [JsonProperty("ActionDescription")]
        public string ActionDescription { get; set; }
    }

    internal class BalanceDataResponse
    {
        [JsonProperty("FundsAvailableResult")]
        public FundsAvailableResult FundsAvailableResult { get; set; }
    }

    internal class FundsAvailableResult
    {
        [JsonProperty("FundsAvailableDateTime")]
        public string FundsAvailableDateTime { get; set; }

        [JsonProperty("FundsAvailable")]
        public string FundsAvailable { get; set; }

        [JsonProperty("BalanceAmount")]
        public string BalanceAmount { get; set; }
    }
}