namespace FintechApp.Responses.OperatorUserwise
{
    public class OperatorUserwiseResponse
    {
        public int OperatorUserwiseID { get; set; }
        public int UserID { get; set; }
        public int OperatorID { get; set; }
        public bool IsActive { get; set; }
        public int UsageLimit { get; set; }
        public int ServiceID { get; set; }
        public string? BlockedAmount { get; set; }
        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

    }
}

