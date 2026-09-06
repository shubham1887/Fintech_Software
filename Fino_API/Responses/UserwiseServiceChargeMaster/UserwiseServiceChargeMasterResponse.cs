namespace FintechApp.Responses.UserwiseServiceChargeMaster {
    public class UserwiseServiceChargeMasterResponse {
        public int UserwiseServiceChargeMasterID { get; set; }
        public int ServiceID { get; set; }
        public int UserID { get; set; }
        public decimal OneTime { get; set; }
        public bool IsDelete { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
