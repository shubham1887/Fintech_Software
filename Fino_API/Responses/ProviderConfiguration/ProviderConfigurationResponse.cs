namespace FintechApp.Responses.ProviderConfiguration
{
    public class ProviderConfigurationResponse
    {
        public int ConfigurationID { get; set; }
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string OpCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Lapu { get; set; } = string.Empty;
        public string Quantity { get; set; } = string.Empty;
        public decimal AdminComm { get; set; }
        public string AdminCommType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string CommissionCategory { get; set; } = string.Empty;
        public int PendingCount { get; set; }
        public int PendingInMinute { get; set; }
        public int FailCount { get; set; }
        public int FailInMinute { get; set; }
        public DateTime DateTime { get; set; }
    }
}
