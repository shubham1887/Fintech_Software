namespace FintechApp.Responses.UserWisePayementGatewaySetting {
    public class UserWisePayementGatewaySettingResponse {
        public int UserWisePayementGatewaySettingID { get; set; }
        public int UserID { get; set; }
        public int PaymentGatewaySettingID { get; set; }
        public decimal MinimumAmount { get; set; }
        public decimal MaximumAmount { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
