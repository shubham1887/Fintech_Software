namespace FintechApp.Responses.RangeWiseSequence
{
    public class RangeWiseSequenceResponse
    {
        public int RangeWiseSequenceID { get; set; }
        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public bool IsDelete { get; set; }
        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

    }
}

