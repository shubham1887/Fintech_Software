using Project.Processing;

namespace Project.Response.ThirdPartyServiceProvider
{
    public class ViewModel
    {
        private int _ThirdPartyServiceProviderID;

        public int ThirdPartyServiceProviderID
        {
            get => _ThirdPartyServiceProviderID;
            set => _ThirdPartyServiceProviderID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();

            set => _Name = value.TrimIfNotNull();
        }

        private bool _IsActiveForWebsite;

        public bool IsActiveForWebsite
        {
            get => _IsActiveForWebsite;
            set => _IsActiveForWebsite = value;
        }

        private bool _IsActive;

        public bool IsActive
        {
            get => _IsActive;
            set => _IsActive = value;
        }

        private string _Token;

        public string Token
        {
            get => _Token.TrimIfNotNull();
            set => _Token = value.TrimIfNotNull();
        }

        private string _AppID;

        public string AppID
        {
            get => _AppID.TrimIfNotNull();
            set => _AppID = value.TrimIfNotNull();
        }

        private string _DomainName;

        public string DomainName
        {
            get => _DomainName.TrimIfNotNull();
            set => _DomainName = value.TrimIfNotNull();
        }

        private string _AllowedServices;

        public string AllowedServices
        {
            get => _AllowedServices.TrimIfNotNull();
            set => _AllowedServices = value.TrimIfNotNull();
        }

        public string IsActiveStr
        {
            get => (IsActive) ? "Yes" : "No";
        }
    }
}