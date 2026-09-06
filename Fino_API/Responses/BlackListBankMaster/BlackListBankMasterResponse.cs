namespace FintechApp.Responses.BlackListBankMaster {
    public class BlackListBankMasterResponse {
        public int BlackListBankMasterID { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
    }
}
