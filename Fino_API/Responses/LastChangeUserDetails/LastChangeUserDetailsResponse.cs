namespace FintechApp.Responses.LastChangeUserDetails {
    public class LastChangeUserDetailsResponse {
        public int LastChangeUserDetailsID { get; set; }
        public int OnboardingUserID { get; set; }
        public string Change { get; set; } = string.Empty;
        public string OldChanges { get; set; } = string.Empty;
        public string NewChanges { get; set; } = string.Empty;
    }
}
