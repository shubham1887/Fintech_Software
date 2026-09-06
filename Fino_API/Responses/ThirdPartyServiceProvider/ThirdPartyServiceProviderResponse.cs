namespace FintechApp.Responses.ThirdPartyServiceProvider {
    public class ThirdPartyServiceProviderResponse {
        public int ThirdPartyServiceProviderID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string DomainName { get; set; } = string.Empty;
        public string AppID { get; set; } = string.Empty;
        public string AllowedServices { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
