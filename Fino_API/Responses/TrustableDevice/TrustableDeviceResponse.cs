namespace FintechApp.Responses.TrustableDevice {
    public class TrustableDeviceResponse {
        public int ID { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public int UserID { get; set; }
        public bool IsVerified { get; set; }
        public string Browser { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
