using Project.Processing;

namespace Project.Requests.Onboarding
{
    public class OnboardingUserDocumentRequest
    {
        private int _OnboardingUserDocumentID;
        public int OnboardingUserDocumentID
        {
            get => _OnboardingUserDocumentID;
            set => _OnboardingUserDocumentID = value;
        }

        private string _Ref_ID;
        public string Ref_ID
        {
            get => _Ref_ID.TrimIfNotNull();
            set => _Ref_ID = value.TrimIfNotNull();
        }
        private string _Type;
        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }
        private string _OldImage;
        public string OldImage
        {
            get => _OldImage.TrimIfNotNull();
            set => _OldImage = value.TrimIfNotNull();
        }
        private string _NewImage;
        public string NewImage
        {
            get => _NewImage.TrimIfNotNull();
            set => _NewImage = value.TrimIfNotNull();
        }
        private string _OnboardingUserID;
        public string OnboardingUserID
        {
            get => _OnboardingUserID.TrimIfNotNull();
            set => _OnboardingUserID = value.TrimIfNotNull();
        }

    }
}
