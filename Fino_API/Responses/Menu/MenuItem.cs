using Newtonsoft.Json;

namespace FintechApp.Responses.Menu
{
    // Output node returned to the client. usertypeid is intentionally NOT exposed — the menu is
    // already filtered server-side for the authenticated role, so role gating never reaches the client.
    public class MenuItem
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string? Url { get; set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string? Icon { get; set; }

        [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
        public List<MenuItem>? Children { get; set; }
    }
}
