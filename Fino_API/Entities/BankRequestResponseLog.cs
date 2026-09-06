namespace FintechApp.Entities
{
    public class BankRequestResponseLog : BaseEntity
    {
        public int BankRequestResponseLogID { get; set; }
        public int UserID { get; set; }
        public int ProviderID { get; set; }
        public int TransactionID { get; set; }
        public string ConsumerNumber { get; set; } = string.Empty;
        public string Request { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Through { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public UserMaster UserMaster { get; set; } = null!;
    }
}
