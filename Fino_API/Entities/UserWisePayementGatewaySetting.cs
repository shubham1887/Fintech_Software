namespace FintechApp.Entities
{
    public class UserWisePayementGatewaySetting : BaseEntity
    {
        public int UserWisePayementGatewaySettingID { get; set; }
        public int UserID { get; set; }
        public int PaymentGatewaySettingID { get; set; }
        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }
        public UserMaster UserMaster { get; set; } = null!;
        public PaymentGatewaySetting PaymentGatewaySetting { get; set; } = null!;
    }
}
