using Project.Processing;

namespace Project.Response.Onboarding
{
    public class LastChangeUserDetailsViewModel
    {

        private int _LastChangeUserDetailsID;
        public int LastChangeUserDetailsID
        {
            get => _LastChangeUserDetailsID;
            set => _LastChangeUserDetailsID = value;
        }
        private int _OnboardingUserID;
        public int OnboardingUserID
        {
            get => _OnboardingUserID;
            set => _OnboardingUserID = value;
        }

        
        private string _Change;
        public string Change
        {
            get => _Change.TrimIfNotNull();
            set => _Change = value.TrimIfNotNull();
        }
        private string _OldChanges;
        public string OldChanges
        {
            get => _OldChanges.TrimIfNotNull();
            set => _OldChanges = value.TrimIfNotNull();
        }
        private string _NewChanges;
        public string NewChanges
        {
            get => _NewChanges.TrimIfNotNull();
            set => _NewChanges = value.TrimIfNotNull();
        }
        private string _FirmName;
        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }
        private string _DOC;
        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }
    }
}
