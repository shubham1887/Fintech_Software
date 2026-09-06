using Project.Processing;

namespace Project.Response.ProviderUserMapping
{
    public class ViewModel
    {
        private int _ProviderUserMappingID;

        public int ProviderUserMappingID
        {
            get => _ProviderUserMappingID;
            set => _ProviderUserMappingID = value;
        }
        private int _UserID;
        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
        private int _ProviderID;
        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _Key;
        public string Key
        {
            get => _Key.TrimIfNotNull();
            set => _Key = value.TrimIfNotNull();
        }
        private string _Value;
        public string Value
        {
            get => _Value.TrimIfNotNull();
            set => _Value = value.TrimIfNotNull();
        }
    }
}
