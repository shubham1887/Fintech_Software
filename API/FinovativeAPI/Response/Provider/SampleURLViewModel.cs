using Project.Processing;

namespace Project.Response.Provider
{
    public class SampleURLViewModel
    {
        private string _DeveloperName;

        public string DeveloperName
        {
            get => _DeveloperName.TrimIfNotNull();
            set => _DeveloperName = value.TrimIfNotNull();
        }

        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private string _URL;

        public string URL
        {
            get => _URL.TrimIfNotNull();
            set => _URL = value.TrimIfNotNull();
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

        private string _HeaderJSON;

        public string HeaderJSON
        {
            get => _HeaderJSON.TrimIfNotNull();
            set => _HeaderJSON = value.TrimIfNotNull();
        }

        private string _DateFormat;

        public string DateFormat
        {
            get => _DateFormat.TrimIfNotNull();
            set => _DateFormat = value.TrimIfNotNull();
        }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }
    }
}