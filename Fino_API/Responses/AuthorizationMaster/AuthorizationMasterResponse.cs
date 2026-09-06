namespace FintechApp.Responses.AuthorizationMaster {
    public class AuthorizationMasterResponse {
        public int AuthorizationMasterID { get; set; }
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int UserTypeID { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}
