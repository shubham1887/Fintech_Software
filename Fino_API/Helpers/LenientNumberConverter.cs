using Newtonsoft.Json;

namespace FintechApp.Helpers
{
    /// <summary>
    /// Reads a numeric request field that arrived as an EMPTY STRING (or null) as the type's
    /// default instead of throwing.
    ///
    /// Every endpoint deserialises its request with <c>JsonConvert.DeserializeObject&lt;T&gt;</c>
    /// inside a try/catch that can only answer "An error occurred while processing your request."
    /// A browser form that leaves an optional dropdown blank sends <c>"SubModeID": ""</c>, and
    /// Newtonsoft's default behaviour is to throw on that — so a perfectly ordinary form (Fund
    /// Request with a Mode that has no sub modes) failed with a message nobody could act on, and
    /// nothing was written to the log either. One unanswered picker should mean "not supplied",
    /// not "the server broke".
    ///
    /// Registered through <see cref="JsonConvert.DefaultSettings"/> in Program.cs, so it applies
    /// to every controller without each one repeating the guard. Values that ARE present still
    /// parse exactly as before — this only decides what a blank means.
    /// </summary>
    public class LenientNumberConverter : JsonConverter
    {
        private static readonly HashSet<Type> Handled = new()
        {
            typeof(int), typeof(int?),
            typeof(long), typeof(long?),
            typeof(short), typeof(short?),
            typeof(decimal), typeof(decimal?),
            typeof(double), typeof(double?),
            typeof(float), typeof(float?),
        };

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => Handled.Contains(objectType);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            Type target = Nullable.GetUnderlyingType(objectType) ?? objectType;
            bool nullable = Nullable.GetUnderlyingType(objectType) != null;

            object? raw = reader.Value;

            // null / missing
            if (reader.TokenType == JsonToken.Null || raw == null)
                return nullable ? null : Activator.CreateInstance(target);

            // "" or "   " — an unanswered picker, not a number
            if (raw is string s)
            {
                string text = s.Trim();
                if (text.Length == 0)
                    return nullable ? null : Activator.CreateInstance(target);

                // A numeric string the sender quoted ("12"); parse it with the invariant rules the
                // rest of the API uses rather than letting the default converter guess.
                if (decimal.TryParse(text, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal parsed))
                    return Convert.ChangeType(parsed, target, System.Globalization.CultureInfo.InvariantCulture);

                // Not a number at all — keep the original error so a genuinely bad payload is
                // still reported rather than silently read as zero.
                throw new JsonSerializationException($"Could not convert '{s}' to {target.Name}.");
            }

            return Convert.ChangeType(raw, target, System.Globalization.CultureInfo.InvariantCulture);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            => throw new NotSupportedException();
    }
}
