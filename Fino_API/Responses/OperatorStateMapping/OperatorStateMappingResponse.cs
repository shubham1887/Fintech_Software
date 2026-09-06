namespace FintechApp.Responses.OperatorStateMapping
{
    public class OperatorStateMappingResponse
    {
        public int OperatorStateMappingID { get; set; }
        public int OperatorID { get; set; }
        public string? OperatorName { get; set; }
        public int ZoneMasterID { get; set; }
        public string? HLR { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }
    }
}
