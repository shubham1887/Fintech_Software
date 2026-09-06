using Project.Processing;

namespace Project.Response.RedirectUrl
{
    public class ViewModel
    {
        private string _Url;

        public string Url
        {
            get => _Url.TrimIfNotNull();
            set => _Url = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }
    }
}