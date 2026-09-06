using Project.Processing;

namespace Project.Response.Sequence
{
    public class RealProviderList
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _ProviderName;
        public string ProviderName { get => _ProviderName.TrimIfNotNull(); set => _ProviderName = value.TrimIfNotNull(); }
    }
}