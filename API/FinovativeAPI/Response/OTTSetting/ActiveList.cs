using Project.Processing;

namespace Project.Response.OTTSetting
{
    public class ActiveList
    {
        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _DisplayName;

        public string DisplayName
        {
            get => _DisplayName.TrimIfNotNullWithDefault("OTT");
            set => _DisplayName = value.TrimIfNotNullWithDefault("OTT");
        }
    }
}