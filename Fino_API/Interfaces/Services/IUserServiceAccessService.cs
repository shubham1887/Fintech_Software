namespace FintechApp.Interfaces.Services
{
    /// <summary>How a service stands for one user, as set on the Add / Edit User services grid.</summary>
    public enum ServiceAccess
    {
        /// <summary>"On" — granted and usable.</summary>
        Granted = 0,
        /// <summary>"Lock" — granted but suspended: still listed, transactions refused.</summary>
        Locked = 1,
        /// <summary>"Off" — not granted: hidden from the catalog / menu and refused.</summary>
        NotGranted = 2,
        /// <summary>
        /// Granted, but withheld until KYC is approved. The platform enforces KYC (Key Setting
        /// "IsKYCEnforced") and this service is not one of the ones the admin allows without it.
        /// Behaves like <see cref="Locked"/> — still listed, transactions refused — but the user
        /// can clear it themselves by completing their KYC.
        /// </summary>
        KycLocked = 3,
        /// <summary>
        /// Granted and otherwise usable, but the platform is inside its scheduled system-down
        /// window (Key Settings "System Down") and this service is not one of the ones the
        /// operator allowed to keep running.
        ///
        /// Distinct from <see cref="Locked"/> on purpose: nothing is wrong with this user or this
        /// service, the whole platform is closed for a stated period, and that is what the user
        /// needs to be told. "Locked by your service provider" would send them to support.
        /// </summary>
        SystemDown = 4,
    }

    /// <summary>
    /// Everything that decides what one user may do with services, resolved once. Holding it in a
    /// single object means a caller that checks many services (the catalog, the sidebar menu) pays
    /// for the lookups once instead of per service.
    /// </summary>
    public sealed class ServiceAccessContext
    {
        /// <summary>ServiceID → enabled, for granted services. null = unrestricted (admin/support,
        /// or a legacy account with no grant rows at all).</summary>
        public Dictionary<int, bool>? Grants { get; init; }

        /// <summary>True when this user's KYC is not approved and the platform enforces KYC.</summary>
        public bool KycBlocked { get; init; }

        /// <summary>Services the admin allows while KYC is still pending.</summary>
        public HashSet<int> KycExemptServiceIDs { get; init; } = new();

        /// <summary>
        /// The scheduled maintenance window as it stands for this request. Open (blocking
        /// nothing) outside the window and for roles that do not transact.
        /// </summary>
        public SystemDownState SystemDown { get; init; } = SystemDownState.Open;

        /// <summary>Nothing to check — the fully unrestricted context (admin / support).</summary>
        public static ServiceAccessContext Unrestricted => new();
    }

    /// <summary>
    /// Resolves the per-user service grants (UserServiceMapping) written by the Add / Edit User
    /// screen, so the On / Lock / Off choice made there actually governs what the user sees and
    /// what they may transact — and layers the platform KYC gate on top of it.
    /// </summary>
    public interface IUserServiceAccessService
    {
        /// <summary>
        /// ServiceID → is enabled, for the user's granted services. Returns null when the user is
        /// unrestricted — admin / support, or a legacy account with no grant rows at all.
        /// </summary>
        Task<Dictionary<int, bool>?> GetGrants(int userID);

        /// <summary>Grants + KYC gate for this user, resolved in one pass.</summary>
        Task<ServiceAccessContext> GetContext(int userID);

        /// <summary>Where one service stands for this user.</summary>
        Task<ServiceAccess> GetAccess(int userID, int serviceID);

        /// <summary>
        /// StatusType.Ok when the user may transact on the service, otherwise the message to show.
        /// Called on the transaction path so a locked / withheld / KYC-pending service can't be
        /// used directly.
        /// </summary>
        Task<string> EnsureUsable(int userID, int serviceID);
    }
}
