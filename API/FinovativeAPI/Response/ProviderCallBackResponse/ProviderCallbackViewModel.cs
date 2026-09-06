using Project.Processing;

namespace Project.Response.ProviderCallBackResponse
{
    public class ProviderCallbackViewModel
    {
        private string _DOC;

        public string DOC
        {
            get => _DOC.TrimIfNotNull();
            set => _DOC = value.TrimIfNotNull();
        }

        private string _ProviderName;
        public string ProviderName { get => _ProviderName.TrimIfNotNull(); set => _ProviderName = value.TrimIfNotNull(); }
        private string _ProviderReply;
        public string ProviderReply { get => _ProviderReply; set => _ProviderReply = value; }
    }
}