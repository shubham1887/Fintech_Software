namespace FintechApp.Responses.OperatorSequence
{
    public class OperatorSequenceResponse
    {
        public int OperatorSequenceID { get; set; }
        public int ServiceID { get; set; }
        public string? ServiceName { get; set; }
        public int OperatorID { get; set; }
        public string? OperatorName { get; set; }
        public int ProviderID { get; set; }
        public string? ProviderName { get; set; }
        public int Count { get; set; }
        public DateTime DateTime { get; set; }
    }
}
