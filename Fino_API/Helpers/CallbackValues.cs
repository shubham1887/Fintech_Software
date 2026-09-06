using Newtonsoft.Json.Linq;

namespace FintechApp.Helpers
{
    /// <summary>
    /// Reads a provider push into one bag, whatever transport it arrived on.
    ///
    /// Providers document one shape and ship another: the same upstream sends a GET query string in
    /// UAT, a form post in production and JSON after a "minor" release. Collecting all three into a
    /// single case-insensitive dictionary means a transport change on their side cannot break
    /// settlement on ours, and one parser per provider reads fields by name without caring how they
    /// travelled.
    /// </summary>
    public static class CallbackValues
    {
        /// <returns>The merged values (query, then form, then body — later wins) and the raw body.</returns>
        public static async Task<(Dictionary<string, string> Values, string RawBody)> Read(HttpRequest request)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in request.Query) values[pair.Key] = pair.Value.ToString();

            if (request.HasFormContentType)
            {
                foreach (var pair in request.Form) values[pair.Key] = pair.Value.ToString();
            }

            string body = string.Empty;
            if (!request.HasFormContentType && request.ContentLength > 0)
            {
                using var reader = new StreamReader(request.Body);
                body = await reader.ReadToEndAsync();
            }

            if (!string.IsNullOrWhiteSpace(body)) Merge(values, body);

            return (values, body);
        }

        // A JSON object first; anything else is treated as a urlencoded body. Nested JSON is
        // flattened one level with a dotted name ("data.status"), which is enough for every push
        // seen so far and keeps the bag flat for the parsers.
        private static void Merge(Dictionary<string, string> values, string body)
        {
            try
            {
                foreach (var property in JObject.Parse(body).Properties())
                {
                    if (property.Value is JObject nested)
                    {
                        foreach (var inner in nested.Properties())
                            values[$"{property.Name}.{inner.Name}"] = inner.Value?.ToString() ?? string.Empty;
                        // Keep the plain name too, so a parser can read the sub-object verbatim.
                    }
                    values[property.Name] = property.Value?.ToString() ?? string.Empty;
                }
                return;
            }
            catch { /* not JSON — fall through to urlencoded */ }

            foreach (string part in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                int split = part.IndexOf('=');
                if (split <= 0) continue;
                values[Uri.UnescapeDataString(part[..split])] = Uri.UnescapeDataString(part[(split + 1)..]);
            }
        }

        /// <summary>First non-empty value among <paramref name="keys"/> — providers name the same
        /// field five different ways, so every parser reads through an alias list.</summary>
        public static string First(IDictionary<string, string> values, params string[] keys)
        {
            foreach (string key in keys)
            {
                if (values.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }
            return string.Empty;
        }
    }
}
