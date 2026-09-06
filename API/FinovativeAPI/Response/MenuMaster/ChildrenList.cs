using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.MenuMaster
{
    public class ChildrenList
    {
        private string _Name;

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Url;

        [JsonProperty(PropertyName = "url")]
        public string Url
        {
            get => _Url.TrimIfNotNull();
            set => _Url = value.TrimIfNotNull();
        }

        private string _Icon;

        [JsonProperty(PropertyName = "icon")]
        public string Icon
        {
            get => _Icon.TrimIfNotNull();
            set => _Icon = value.TrimIfNotNull();
        }

        private decimal _MenuSequence;
        public decimal MenuSequence { get => _MenuSequence; set => _MenuSequence = value; }
    }
}