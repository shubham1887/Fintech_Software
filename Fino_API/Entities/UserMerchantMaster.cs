namespace FintechApp.Entities
{
    public class UserMerchantMaster : BaseEntity
    {
        public int UserMerchantMasterID { get; set; }
        public int UserID { get; set; }
        public string PaySprintUPIMerchantCode { get; set; } = string.Empty;
        public string PaySprintStaticQR { get; set; } = string.Empty;
        public string PaySprintUPIRefid { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
