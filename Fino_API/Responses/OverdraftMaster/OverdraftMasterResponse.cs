namespace FintechApp.Responses.OverdraftMaster {
    public class OverdraftMasterResponse {
        public int OverdraftMasterID { get; set; }
        public int UserID { get; set; }
        public decimal Overdraft { get; set; }
        public decimal MaxOverdraft { get; set; }
        public int WalletID { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
