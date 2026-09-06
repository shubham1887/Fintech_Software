namespace FintechApp.Entities
{
    public class UpiSender : BaseEntity
    {
        public int UpiSenderID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string GUID { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
