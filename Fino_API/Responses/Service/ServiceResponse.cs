namespace FintechApp.Responses.Service
{
    public class ServiceResponse
    {
        public int ServiceID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ServiceCategory { get; set; } = string.Empty;
        public string ServiceImagePath { get; set; } = string.Empty;
        public int UISequence { get; set; }
        public string HSNCode { get; set; } = string.Empty;
        public string HSNCodeForSelfInvoice { get; set; } = string.Empty;
        public decimal Charge { get; set; }
        public decimal MinBalance { get; set; }
        public bool IsActive { get; set; }
        public bool IsDisplay { get; set; }
        public bool IsDefault { get; set; }
        public bool IsOutside { get; set; }
        public bool? IsBBPS { get; set; }
        public bool IsSubscriptionAllow { get; set; }
        public bool IsGSTInclusive { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
