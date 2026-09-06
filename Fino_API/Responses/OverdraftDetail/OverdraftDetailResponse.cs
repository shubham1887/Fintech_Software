namespace FintechApp.Responses.OverdraftDetail {
    public class OverdraftDetailResponse {
        public int OverdraftDetailID { get; set; }
        public int UserID { get; set; }
        public int WalletID { get; set; }
        public decimal OverdraftOpening { get; set; }
        public decimal OverdraftAmount { get; set; }
        public decimal OverdraftClosing { get; set; }
        public string UserRemark { get; set; } = string.Empty;
        public string AdminRemark { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal OpeningWalletBalance { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
