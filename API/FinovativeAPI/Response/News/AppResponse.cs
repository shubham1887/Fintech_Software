using Project.Processing;

namespace Project.Response.News
{
    public class AppResponse
    {
        private string _HyperLink;

        public string HyperLink
        {
            get => _HyperLink.TrimIfNotNull();
            set => _HyperLink = value.TrimIfNotNull();
        }

        private string _Title;

        public string Title
        {
            get => _Title.TrimIfNotNull();
            set => _Title = value.TrimIfNotNull();
        }
    }
}