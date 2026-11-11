namespace FintechApp.Responses.SystemDown
{
    /// <summary>One service as the maintenance-window screen lists it.</summary>
    public class SystemDownServiceItem
    {
        public int ServiceID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ServiceCategory { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        /// <summary>This service keeps running while the window is open.</summary>
        public bool IsAllowedWhenSystemDown { get; set; }
    }

    /// <summary>
    /// The maintenance window as the admin screen reads it: the configuration, plus whether the
    /// window happens to be open at this moment (so the screen can say "down now" rather than
    /// leaving the operator to compare the hours against their own clock).
    /// </summary>
    public class SystemDownPolicyResponse
    {
        public bool IsSystemDown { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        /// <summary>True when the switch is on AND the clock is inside the window right now.</summary>
        public bool IsDownNow { get; set; }
        public List<SystemDownServiceItem> Services { get; set; } = new();
    }

    /// <summary>
    /// What every signed-in client asks for to decide whether to show a maintenance banner. Small
    /// and cheap on purpose - it is polled by every open page, not read once at sign-in, because a
    /// window that starts at 22:00 has to reach a page that was opened at 21:50.
    /// </summary>
    public class SystemDownStatusResponse
    {
        public bool IsDownNow { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        /// <summary>Names of the services still running, for the banner to list.</summary>
        public List<string> AllowedServices { get; set; } = new();
    }
}
