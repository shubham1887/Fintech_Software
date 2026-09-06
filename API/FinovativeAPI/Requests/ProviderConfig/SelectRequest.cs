namespace Project.Requests.ProviderConfig
{
    public class SelectRequest
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }
    }
}