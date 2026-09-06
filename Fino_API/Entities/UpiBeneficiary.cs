namespace FintechApp.Entities
{
    public class UpiBeneficiary : BaseEntity
    {
        public int UpiBeneficiaryID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UpiID { get; set; } = string.Empty;
        public string GUID { get; set; } = string.Empty;
        public string SenderMobile { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public int UpiSenderID { get; set; }
        public int UserID { get; set; }
        public UpiSender UpiSender { get; set; } = null!;
        public UserMaster UserMaster { get; set; } = null!;
    }
}
