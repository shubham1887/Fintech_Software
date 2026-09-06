using Project.Processing;

namespace Project.Response.URLType
{
    public class ViewModel
    {
        private int _UrlTypeID;

        public int UrlTypeID
        {
            get => _UrlTypeID;
            set => _UrlTypeID = value;
        }

        private string _UrlType;

        public string UrlType
        {
            get => _UrlType.TrimIfNotNull();
            set => _UrlType = value.TrimIfNotNull();
        }
    }
}