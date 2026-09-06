namespace FintechApp.Responses.UpiBeneficiary {
    public class UpiBeneficiaryResponse {
        public int UpiBeneficiaryID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UpiID { get; set; } = string.Empty;
        public string GUID { get; set; } = string.Empty;
        public string SenderMobile { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public int UpiSenderID { get; set; }
        public int UserID { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
