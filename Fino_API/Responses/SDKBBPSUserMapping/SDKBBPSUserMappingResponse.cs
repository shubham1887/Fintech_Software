namespace FintechApp.Responses.SDKBBPSUserMapping {
    public class SDKBBPSUserMappingResponse {
        public int SDKBBPSUserMappingID { get; set; }
        public int UserID { get; set; }
        public string ConfirmationCallbackURL { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
