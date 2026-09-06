namespace FintechApp.Responses.ProviderResponseMaster
{
    public class ProviderResponseMasterResponse
    {
        public int ProviderResponseMasterID { get; set; }
        public int ProviderID { get; set; }
        public string TextMustexits { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public bool IsReference { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

    }
}

