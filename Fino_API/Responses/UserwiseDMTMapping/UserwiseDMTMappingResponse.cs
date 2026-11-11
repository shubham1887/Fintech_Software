namespace FintechApp.Responses.UserwiseDMTMapping
{
    // The sequence is a routing rule: "for THIS user, THIS operator goes through THIS provider".
    // Ids alone are unreadable on screen, so the names travel with them — the repository already
    // includes the four navigation properties, so this costs no extra query.
    public class UserwiseDMTMappingResponse
    {
        public int UserwiseDMTMappingID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FirmName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public int ServiceID { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public int ProviderID { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
