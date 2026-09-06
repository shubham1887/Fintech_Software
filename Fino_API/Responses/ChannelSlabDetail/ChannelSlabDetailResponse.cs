namespace FintechApp.Responses.ChannelSlabDetail
{
    public class ChannelSlabDetailResponse
    {
        public int ChannelSlabDetailID { get; set; }
        public int ChannelSlabMasterID { get; set; }
        public string SlabName { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public decimal Commission { get; set; }
        public string CommissionType { get; set; } = string.Empty;
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
        public int UplineUserID { get; set; }
        public decimal RealCommission { get; set; }
        public string RealCommissionType { get; set; } = string.Empty;
        public string GeneralCommissionCategory { get; set; } = string.Empty;
        public string RealCommissionCategory { get; set; } = string.Empty;
        public decimal UplineCommission { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }
    }
}
