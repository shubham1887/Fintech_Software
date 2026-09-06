using Project.Processing;

namespace Project.Response.User
{
    public class UISettingsViewModel
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