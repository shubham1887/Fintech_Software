using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Globalization;

namespace Project.Response.SupportUserAccessDetail
{
    public partial class Parent
    {
        [JsonProperty("menumaterid")]
        public int MenuMasterID { get; set; }

        [JsonProperty("usertypeid")]
        public List<int> Usertypeid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("children")]
        public List<Child> Children = new List<Child>();

        [JsonProperty("IsActive")]
        public bool IsActive { get; set; }
    }

    public partial class Child
    {
        [JsonProperty("menumaterid")]
        public int MenuMasterID { get; set; }

        [JsonProperty("usertypeid")]
        public List<int> Usertypeid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("IsActive")]
        public bool IsActive { get; set; }
    }

    public partial class Parent
    {
        public static Parent FromJson(string json) => JsonConvert.DeserializeObject<Parent>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Parent self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}