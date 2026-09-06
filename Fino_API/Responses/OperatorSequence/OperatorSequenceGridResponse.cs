namespace FintechApp.Responses.OperatorSequence
{
    // Provider cell of the sequence grid: every PAY-capable provider of the service,
    // IsActive marks membership in the operator's sequence, Count is its priority weight.
    public class SequenceProviderItemResponse
    {
        public int OperatorID { get; set; }
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public int Count { get; set; }
        public bool IsActive { get; set; }
    }

    // One grid row per operator (mirrors the legacy OperatorSequenceResponse shape the UI expects).
    public class OperatorSequenceGridResponse
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public List<SequenceProviderItemResponse> ProviderList { get; set; } = new();
        public int RealProviderID { get; set; }
        public int ViewProviderID { get; set; }
        public int OperatorSequenceUserWiseID { get; set; }
    }

    public class SequenceProviderDropdownResponse
    {
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
    }
}
