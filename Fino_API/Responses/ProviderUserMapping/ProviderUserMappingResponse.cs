namespace FintechApp.Responses.ProviderUserMapping
{
    public class ProviderUserMappingResponse
    {
        public int ProviderUserMappingID { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int ProviderID { get; set; }
        public int UserID { get; set; }
        public DateTime DateTime { get; set; }

        public DateTime UpdatedateTime { get; set; }

    }
}

