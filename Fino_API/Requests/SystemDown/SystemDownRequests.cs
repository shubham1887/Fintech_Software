namespace FintechApp.Requests.SystemDown
{
    /// <summary>
    /// The whole maintenance window, saved as one decision. The switch, the hours and the
    /// exception list arrive together because they only mean anything together - saving "on"
    /// without the hours, or the hours without the exceptions, leaves a half-configured window
    /// that is live.
    /// </summary>
    public class SystemDownSaveRequest
    {
        /// <summary>Master switch. Off leaves the hours and exceptions stored but inert.</summary>
        public bool IsSystemDown { get; set; }

        /// <summary>Window start, 24-hour "HH:mm". Blank = no start bound.</summary>
        public string From { get; set; } = string.Empty;

        /// <summary>Window end, 24-hour "HH:mm". Earlier than From crosses midnight.</summary>
        public string To { get; set; } = string.Empty;

        /// <summary>What users are told while the window is open. Blank falls back to a standard wording.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>ServiceIDs that keep running through the window. Everything else is refused.</summary>
        public List<int> AllowedServiceIDs { get; set; } = new();
    }

    /// <summary>
    /// Carries nothing. The two read endpoints take no parameters, but the envelope every call in
    /// this API uses has a Request field, so they need a type to deserialise it into.
    /// </summary>
    public class SystemDownReadRequest
    {
    }
}
