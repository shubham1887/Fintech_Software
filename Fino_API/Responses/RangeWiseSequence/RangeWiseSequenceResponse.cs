namespace FintechApp.Responses.RangeWiseSequence
{
    // Amount-band routing for DMT: transfers between FromAmount and ToAmount for an operator go
    // through the named provider. Names ride along with the ids so the screen is readable.
    public class RangeWiseSequenceResponse
    {
        public int RangeWiseSequenceID { get; set; }
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public bool IsDelete { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
