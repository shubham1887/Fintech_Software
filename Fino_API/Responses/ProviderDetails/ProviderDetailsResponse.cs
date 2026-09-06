namespace FintechApp.Responses.ProviderDetails
{
    public class ProviderDetailsResponse
    {
        public int ProviderDetailID { get; set; }
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string UrlType { get; set; } = string.Empty;
        public string ProviderURL { get; set; } = string.Empty;
        public string MethodType { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string HeaderJson { get; set; } = string.Empty;
        public string DateFormat { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string TransactionIDPrefix { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
    }
}
