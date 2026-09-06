using Project.Processing;

namespace Project.Response.ProviderConfig
{
    public class DeveloperViewModel
    {
        private string _Developer;

        public string Developer
        {
            get => _Developer.TrimIfNotNull();
            set => _Developer = value.TrimIfNotNull();
        }
    }
}