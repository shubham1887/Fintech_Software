namespace Project.Requests.ProviderResponse
{
    public class ActivateRequest
    {
        private int _ProviderResponseMasterID;
        private bool _IsActive;

        public int ProviderResponseMasterID
        {
            get => _ProviderResponseMasterID;
            set => _ProviderResponseMasterID = value;
        }

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }
    }
}