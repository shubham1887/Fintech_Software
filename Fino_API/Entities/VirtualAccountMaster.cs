namespace FintechApp.Entities
{
    public class VirtualAccountMaster : BaseEntity
    {
        public int VirtualAccountMasterID { get; set; }
        public string Prefix { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
    }
}
