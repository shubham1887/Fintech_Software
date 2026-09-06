using Project.Processing;

namespace Project.Response.SDKBBPSUserMapping
{
    public class ViewModel
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _SDKBBPSUserMappingID;

        public int SDKBBPSUserMappingID
        {
            get => _SDKBBPSUserMappingID;
            set => _SDKBBPSUserMappingID = value;
        }

        private string _ConfirmationCallbackURL;

        public string ConfirmationCallbackURL
        {
            get => _ConfirmationCallbackURL.TrimIfNotNull();
            set => _ConfirmationCallbackURL = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }
    }
}