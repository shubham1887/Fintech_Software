namespace FintechApp.Responses.ProviderOperatorMaster
{
    public class ProviderOperatorMasterResponse
    {
        public int ProviderOperatorID { get; set; }
        public int ProviderID { get; set; }
        public int OperatorID { get; set; }
        public int ServiceID { get; set; }
        public string? Operatorcode { get; set; }
        public string? MarginType { get; set; }
        public decimal CommissionAdmin { get; set; }
        public int PendingCount { get; set; }
        public int FailureCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

    }
}

