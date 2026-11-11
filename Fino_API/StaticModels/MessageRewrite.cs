namespace FintechApp.StaticModels
{
    /// <summary>How a rule's pattern is compared against the provider's message.</summary>
    public static class MessageMatchType
    {
        public const string Contains = "CONTAINS";
        public const string Equals = "EQUALS";
        public const string StartsWith = "STARTSWITH";
        public const string Regex = "REGEX";

        public static readonly string[] All = { Contains, Equals, StartsWith, Regex };

        /// <summary>Normalises whatever the UI sent; anything unrecognised falls back to CONTAINS,
        /// which is the safe reading of "this text appears in the reply".</summary>
        public static string Normalize(string? value)
        {
            string v = (value ?? string.Empty).Trim().ToUpperInvariant();
            return Array.IndexOf(All, v) >= 0 ? v : Contains;
        }
    }

    /// <summary>Who sees the rewritten message.</summary>
    public static class MessageChannel
    {
        /// <summary>Everyone — the panel, the Android app and the B2B API.</summary>
        public const string All = "ALL";
        /// <summary>The panel and the Android app (your own network).</summary>
        public const string Panel = "PANEL";
        /// <summary>The B2B API only (integrators calling /api/v1).</summary>
        public const string Api = "API";

        public static readonly string[] Options = { All, Panel, Api };

        public static string Normalize(string? value)
        {
            string v = (value ?? string.Empty).Trim().ToUpperInvariant();
            return Array.IndexOf(Options, v) >= 0 ? v : All;
        }

        /// <summary>Does a rule scoped to <paramref name="ruleChannel"/> apply on this channel?</summary>
        public static bool Covers(string ruleChannel, string requestChannel)
        {
            string rule = Normalize(ruleChannel);
            string req = Normalize(requestChannel);
            return rule == All || req == All || rule == req;
        }
    }
}
