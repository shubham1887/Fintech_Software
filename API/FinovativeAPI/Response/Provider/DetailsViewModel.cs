using Project.Processing;

namespace Project.Response.Provider
{
    public class DetailsViewModel
    {
        private int _ProviderDetailID;

        public int ProviderDetailID
        {
            get => _ProviderDetailID;
            set => _ProviderDetailID = value;
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _ProviderURL;

        public string ProviderURL
        {
            get => _ProviderURL.TrimIfNotNull();
            set => _ProviderURL = value.TrimIfNotNull();
        }

        private string _MethodType;

        public string MethodType
        {
            get => _MethodType.TrimIfNotNull();
            set => _MethodType = value.TrimIfNotNull();
        }

        private string _HeaderJson;

        public string HeaderJson
        {
            get => _HeaderJson.TrimIfNotNull();
            set => _HeaderJson = value.TrimIfNotNull();
        }

        private string _DateFormat;

        public string DateFormat
        {
            get => _DateFormat.TrimIfNotNull();
            set => _DateFormat = value.TrimIfNotNull();
        }

        private bool _IsDelete;

        public bool IsDelete
        {
            get => _IsDelete;
            set => _IsDelete = value;
        }

        private bool isActive;

        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        private string _UrlType;

        public string UrlType
        {
            get => _UrlType.TrimIfNotNull();
            set => _UrlType = value.TrimIfNotNull();
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _ProviderID;

        public int ProviderID
        {
            get => _ProviderID;
            set => _ProviderID = value;
        }

        private string _TransactionIDPrefix;

        public string TransactionIDPrefix
        {
            get => _TransactionIDPrefix;
            set => _TransactionIDPrefix = value;
        }
    }
}