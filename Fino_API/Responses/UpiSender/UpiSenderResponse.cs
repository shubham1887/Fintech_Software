namespace FintechApp.Responses.UpiSender {
    public class UpiSenderResponse {
        public int UpiSenderID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string GUID { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
