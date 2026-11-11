namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Where the scheduled maintenance window stands right now, resolved once.
    /// </summary>
    public sealed class SystemDownState
    {
        /// <summary>True when the window is switched on AND the clock is inside it.</summary>
        public bool IsDown { get; init; }

        /// <summary>True when the window is switched on at all, whatever the clock says. Lets a
        /// screen show "maintenance is scheduled for 22:00-02:00" outside the window itself.</summary>
        public bool IsScheduled { get; init; }

        /// <summary>Window start as stored, "HH:mm" (empty = no start bound).</summary>
        public string From { get; init; } = string.Empty;

        /// <summary>Window end as stored, "HH:mm" (empty = no end bound).</summary>
        public string To { get; init; } = string.Empty;

        /// <summary>What to tell the user. Never empty while <see cref="IsDown"/>.</summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>Services the operator has allowed to keep running through the window.</summary>
        public HashSet<int> AllowedServiceIDs { get; init; } = new();

        /// <summary>The window is off - nothing is blocked.</summary>
        public static SystemDownState Open => new();

        /// <summary>Is this service refused right now because of the window?</summary>
        public bool Blocks(int serviceID) => IsDown && !AllowedServiceIDs.Contains(serviceID);
    }

    /// <summary>
    /// The scheduled system-down window (Key Settings "System Down"). While it is open every
    /// financial transaction is refused except on the services the operator marked as still
    /// allowed, so that a maintenance window is one decision in one place rather than a service
    /// catalogue switched off by hand and switched back on from memory.
    /// </summary>
    public interface ISystemDowntimeService
    {
        /// <summary>The window's current state, including the allowed-service set.</summary>
        Task<SystemDownState> GetState();

        /// <summary>
        /// StatusType.Ok when this service may transact right now, otherwise the message to show
        /// the user. Called on every transaction entry point.
        /// </summary>
        Task<string> EnsureOpen(int serviceID);

        /// <summary>
        /// The same check for a path that knows only which KIND of service it is - Micro ATM and
        /// AEPS attribute their transactions to a service resolved later, so there is no id to
        /// pass at the point the request has to be accepted or refused.
        ///
        /// Open when at least one active service of the kind is allowed through the window, which
        /// is how an operator's "keep AEPS running" is expressed when AEPS is more than one row in
        /// the catalogue.
        /// </summary>
        Task<string> EnsureOpenForKind(string serviceKind);

        /// <summary>
        /// The check for money that moves without belonging to a service at all - a fund transfer
        /// down the network, or a fund request raised up it. There is no service to exempt, so
        /// these are refused for as long as the window is open.
        /// </summary>
        Task<string> EnsureOpenForMoneyMovement();

        /// <summary>The window and its exception list, for the admin screen that edits them.</summary>
        Task<Responses.SystemDown.SystemDownPolicyResponse> SelectPolicy();

        /// <summary>Save the window and its exception list as one change.</summary>
        Task<string> SavePolicy(Requests.SystemDown.SystemDownSaveRequest request);

        /// <summary>The small answer every signed-in page polls for its maintenance banner.</summary>
        Task<Responses.SystemDown.SystemDownStatusResponse> SelectStatus();
    }
}
