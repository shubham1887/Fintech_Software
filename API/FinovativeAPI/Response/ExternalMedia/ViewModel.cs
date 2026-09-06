using Project.Processing;

namespace Project.Response.ExternalMedia
{
    public class ViewModel
    {
        private string _Key;
        public string Key
        {
            get => _Key.TrimIfNotNull();
            set => _Key = value.TrimIfNotNull();
        }

        private string _Value;
        public string Value
        {
            get => _Value.TrimIfNotNull();
            set => _Value = value.TrimIfNotNull();
        }
      
    }
}
