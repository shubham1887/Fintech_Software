namespace FintechApp.Entities
{
    public class WrongPasswordHistory : BaseEntity
    {
        public int WrongPasswordHistoryID { get; set; }
        public int UserID { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
    }
}
