using Project.Processing;

namespace Project.Response.Provider
{
    public class ProviderSampleViewModel
    {
        private int _ProviderDetailsSampleID;

        public int ProviderDetailsSampleID
        {
            get => _ProviderDetailsSampleID;
            set => _ProviderDetailsSampleID = value;
        }

        private int _UrlTypeID;

        public int UrlTypeID
        {
            get => _UrlTypeID;
            set => _UrlTypeID = value;
        }

        private string _ProviderName;

        public string ProviderName
        {
            get => _ProviderName.TrimIfNotNull();
            set => _ProviderName = value.TrimIfNotNull();
        }

        private string _ProviderURL;

        public string ProviderURL
        {
            get => _ProviderURL.TrimIfNotNull();
            set => _ProviderURL = value.TrimIfNotNull();
        }

        private string _UrlType;

        public string UrlType
        {
            get => _UrlType.TrimIfNotNull();
            set => _UrlType = value.TrimIfNotNull();
        }

        private string _MethodType;

        public string MethodType
        {
            get => _MethodType.TrimIfNotNull();
            set => _MethodType = value.TrimIfNotNull();
        }

        private string _ContentType;

        public string ContentType
        {
            get => _ContentType.TrimIfNotNull();
            set => _ContentType = value.TrimIfNotNull();
        }

        private string _DateFormat;

        public string DateFormat
        {
            get => _DateFormat.TrimIfNotNull();
            set => _DateFormat = value.TrimIfNotNull();
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }
    }
}