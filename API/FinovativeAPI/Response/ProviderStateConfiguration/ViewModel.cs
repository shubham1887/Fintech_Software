using Project.Processing;

namespace Project.Response.ProviderStateConfiguration
{
    public class ViewModel
    {
        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private int _StateID;

        public int StateID
        {
            get => _StateID;
            set => _StateID = value;
        }

        private string _StateName;

        public string StateName
        {
            get => _StateName.TrimIfNotNull();
            set => _StateName = value.TrimIfNotNull();
        }

        private int _ProviderStateConfigurationID;

        public int ProviderStateConfigurationID
        {
            get => _ProviderStateConfigurationID;
            set => _ProviderStateConfigurationID = value;
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private string _StateCode;

        public string StateCode
        {
            get => _StateCode.TrimIfNotNull();
            set => _StateCode = value.TrimIfNotNull();
        }
    }
}