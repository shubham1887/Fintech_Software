namespace FintechApp.Responses.ProviderGateway
{
    // Result of a live balance enquiry against a provider's BALANCE endpoint.
    public class ProviderBalanceResponse
    {
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public decimal? RechargeBalance { get; set; }
        public decimal? DMTBalance { get; set; }
        public Dictionary<string, string> ExtractedValues { get; set; } = new();
        public string RawResponse { get; set; } = string.Empty;
    }

    // Input for a bill-details enquiry (BBPS / utility) before payment.
    public class BillFetchContext
    {
        public int OperatorID { get; set; }
        public int ServiceID { get; set; }
        public string ConsumerNumber { get; set; } = string.Empty;
        public string Param1 { get; set; } = string.Empty;
        public string Param2 { get; set; } = string.Empty;
        public string Param3 { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string IPAddress { get; set; } = string.Empty;
    }

    // Result of running the provider's BILLFETCH endpoint through the BILLFETCH rules.
    public class BillFetchResponse
    {
        public string Status { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public int ProviderID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal? BillAmount { get; set; }
        public string DueDate { get; set; } = string.Empty;
        public string BillDate { get; set; } = string.Empty;
        public string BillNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string RawResponse { get; set; } = string.Empty;
    }

    // Result of dry-running a sample reply through the response rules.
    public class ResponseRuleTestResponse
    {
        public bool Matched { get; set; }
        public int? MatchedRuleID { get; set; }
        public string MatchedPattern { get; set; } = string.Empty;
        public string MappedStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string> ExtractedValues { get; set; } = new();
    }

    // One supported template token, served to the UI so the chip list stays in sync with the engine.
    public class PlaceholderInfo
    {
        public string Token { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;

        // "request" = usable inside endpoint URL/body templates.
        // "capture" = usable inside response rule templates to extract values.
        public string Direction { get; set; } = string.Empty;
    }
}
