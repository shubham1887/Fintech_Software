namespace FintechApp.Responses.UserCreationMaster
{
    public class UserCreationMasterResponse
    {
        public int UserCreationMasterID { get; set; }
        public int UserID { get; set; }
        public int ServiceID { get; set; }
        public string? ServiceName { get; set; }
        public int WalletID { get; set; }
        public int OpeningIdCount { get; set; }
        public int PurchaseCount { get; set; }
        public int ClosingIdCount { get; set; }
        public decimal Amount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public string? Type { get; set; }
        public int CreatedUserID { get; set; }
        public DateTime DateTime { get; set; }
    }
}
