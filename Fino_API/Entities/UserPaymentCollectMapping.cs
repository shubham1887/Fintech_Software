namespace FintechApp.Entities
{
    public class UserPaymentCollectMapping : BaseEntity
    {
        public int UserPaymentCollectMappingID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public int UserID { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
