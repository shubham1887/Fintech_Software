namespace FintechApp.Entities
{
    public class WalletServiceMapping : BaseEntity
    {
        public int WalletServiceMappingID { get; set; }
        public int WalletID { get; set; }
        public int ServiceID { get; set; }
        public WalletMaster WalletMaster { get; set; } = null!;
        public ServiceMaster ServiceMaster { get; set; } = null!;
    }
}
