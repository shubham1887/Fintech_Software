using Project.Processing;

namespace Project.Response.VirtualAccountDetails
{
    public class ViewModel
    {
        private int _VirtualAccountDeatailID;

        public int VirtualAccountDeatailID
        {
            get => _VirtualAccountDeatailID;
            set => _VirtualAccountDeatailID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
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