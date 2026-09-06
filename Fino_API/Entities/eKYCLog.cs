namespace FintechApp.Entities
{
    public class eKYCLog : BaseEntity
    {
        public int eKYCLogID { get; set; }
        public string DocumentNo { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string Response { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
