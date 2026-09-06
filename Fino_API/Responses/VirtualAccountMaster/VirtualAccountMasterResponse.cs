namespace FintechApp.Responses.VirtualAccountMaster {
    public class VirtualAccountMasterResponse {
        public int VirtualAccountMasterID { get; set; }
        public string Prefix { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
