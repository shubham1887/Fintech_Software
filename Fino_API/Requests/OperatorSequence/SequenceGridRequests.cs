namespace FintechApp.Requests.OperatorSequence
{
    // One provider entry inside a sequence payload (mirrors the legacy UI contract).
    public class SequenceProviderItem
    {
        public int OperatorID { get; set; }
        public int ProviderID { get; set; }
        public int Count { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class SelectSequenceByServiceRequest
    {
        public int ServiceID { get; set; }
    }

    // Replaces the full sequence of one operator (legacy OperatorSequence/Insert).
    public class InsertOperatorSequenceRequest
    {
        public int ServiceID { get; set; }
        public int UserID { get; set; }
        public int OperatorID { get; set; }
        public int Count { get; set; }
        public List<SequenceProviderItem> ProviderList { get; set; } = new();
        public int RealProviderID { get; set; }
        public int ViewProviderID { get; set; }
    }

    // One row of the legacy OperatorSequence/BulkUpdate payload.
    // Criteria: AddSequence | RemoveSequence | ViewAPI | RealAPI
    public class BulkUpdateOperatorSequenceRequest
    {
        public string Criteria { get; set; } = string.Empty;
        public string CriteriaText { get; set; } = string.Empty;
        public bool IsTextValue { get; set; }
        public bool CriteriaValue { get; set; }
        public int ServiceID { get; set; }
        public int UserID { get; set; }
        public int OperatorID { get; set; }
        public int Count { get; set; }
        public List<SequenceProviderItem> ProviderList { get; set; } = new();
        public int RealProviderID { get; set; }
        public int ViewProviderID { get; set; }
    }
}
