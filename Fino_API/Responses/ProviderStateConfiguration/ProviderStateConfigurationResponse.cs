namespace FintechApp.Responses.ProviderStateConfiguration
{
    public class ProviderStateConfigurationResponse
    {
        public int ProviderStateConfigurationID { get; set; }
        public int ProviderID { get; set; }
        public int StateID { get; set; }
        public string StateCode { get; set; } = string.Empty;
    }
}
