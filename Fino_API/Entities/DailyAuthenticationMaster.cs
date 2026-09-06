namespace FintechApp.Entities
{
    public class DailyAuthenticationMaster : BaseEntity
    {
        public int AuthenticationID { get; set; }
        public int UserID { get; set; }
        public int ProviderID { get; set; }
        public int ServiceID { get; set; }
        public int OperatorID { get; set; }
        public string Pipe { get; set; } = string.Empty;
        public UserMaster UserMaster { get; set; } = null!;
        public ProviderMaster ProviderMaster { get; set; } = null!;
        public ServiceMaster ServiceMaster { get; set; } = null!;
    }
}
