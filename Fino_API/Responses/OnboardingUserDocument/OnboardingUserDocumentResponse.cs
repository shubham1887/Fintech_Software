namespace FintechApp.Responses.OnboardingUserDocument {
    public class OnboardingUserDocumentResponse {
        public int OnboardingUserDocumentID { get; set; }
        public string Ref_ID { get; set; } = string.Empty;
        public int Type { get; set; }
        public string OldImage { get; set; } = string.Empty;
        public string NewImage { get; set; } = string.Empty;
        public int OnboardingUserID { get; set; }
    }
}
