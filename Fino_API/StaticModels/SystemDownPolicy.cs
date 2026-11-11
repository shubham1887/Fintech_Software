namespace FintechApp.StaticModels
{
    /// <summary>
    /// The scheduled maintenance window: when the platform is "down", and what that means.
    ///
    /// Kept as a pure static so the one tricky part - deciding whether a clock time falls inside a
    /// window that may cross midnight - has a single implementation. A second copy of that
    /// comparison is how a 22:00-to-02:00 window ends up open all night on one screen and closed
    /// all day on another.
    /// </summary>
    public static class SystemDownPolicy
    {
        /// <summary>Key Setting: the master switch.</summary>
        public const string EnabledKey = "IsSystemDown";
        /// <summary>Key Setting: window start, 24-hour "HH:mm".</summary>
        public const string FromKey = "SystemDownFrom";
        /// <summary>Key Setting: window end, 24-hour "HH:mm".</summary>
        public const string ToKey = "SystemDownTo";
        /// <summary>Key Setting: what the user is told while the window is open.</summary>
        public const string MessageKey = "SystemDownMessage";

        /// <summary>Shown when the operator has not written a message of their own.</summary>
        public const string DefaultMessage =
            "The system is temporarily down for maintenance. Transactions will resume shortly.";

        /// <summary>
        /// Parse a stored "HH:mm" (also accepts "H:mm" and "HH:mm:ss") into a time of day.
        /// Returns null for blank or unparseable text, which the callers read as "no bound".
        /// </summary>
        public static TimeSpan? ParseClock(string? text)
        {
            string s = (text ?? string.Empty).Trim();
            if (s.Length == 0) return null;
            if (TimeSpan.TryParse(s, out var parsed) && parsed >= TimeSpan.Zero && parsed < TimeSpan.FromDays(1))
                return parsed;
            return null;
        }

        /// <summary>
        /// Is <paramref name="now"/> inside the window that runs from <paramref name="from"/> to
        /// <paramref name="to"/>?
        ///
        /// The start is inclusive and the end exclusive, so a 22:00-02:00 window is shut at 22:00
        /// sharp and open again at 02:00 sharp - no minute belongs to both states.
        ///
        /// A window whose end is EARLIER than its start crosses midnight, which is when
        /// maintenance is normally run; that case is the reason this is not a plain
        /// "from &lt;= now &lt; to".
        ///
        /// With neither bound set the window is the whole day: the operator has turned the switch
        /// on without naming hours, and the only safe reading of that is "down now". With one
        /// bound set the other is open-ended in that direction.
        /// </summary>
        public static bool IsWithin(TimeSpan now, TimeSpan? from, TimeSpan? to)
        {
            if (from == null && to == null) return true;
            if (from == null) return now < to!.Value;
            if (to == null) return now >= from.Value;
            if (from.Value == to.Value) return true;   // zero-length is read as all day, as above
            return from.Value < to.Value
                ? now >= from.Value && now < to.Value
                : now >= from.Value || now < to.Value; // crosses midnight
        }
    }
}
