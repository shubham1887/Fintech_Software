namespace FintechApp.Responses.WalletServiceMapping {
    public class WalletServiceMappingResponse {
        public int WalletServiceMappingID { get; set; }
        public int WalletID { get; set; }
        public int ServiceID { get; set; }
        public string WalletName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
