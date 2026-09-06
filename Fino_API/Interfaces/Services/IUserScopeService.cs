namespace FintechApp.Interfaces.Services
{
    /// <summary>Outcome of a hierarchy-scope check on one request.</summary>
    public sealed class ScopeCheckResult
    {
        public bool Allowed { get; init; } = true;
        /// <summary>Human-readable "which field pointed where" — for the audit log only.</summary>
        public string Violation { get; init; } = string.Empty;

        public static readonly ScopeCheckResult Ok = new();
        public static ScopeCheckResult Denied(string violation) => new() { Allowed = false, Violation = violation };
    }

    /// <summary>
    /// Answers "may this caller name that user id?" from the UserMaster.ParentID tree, and
    /// enforces it generically over a whole request body.
    /// </summary>
    public interface IUserScopeService
    {
        /// <summary>Enforce (deny), Audit (log only) or Off — from config "HierarchyScope:Mode".</summary>
        bool IsEnforcing { get; }

        /// <summary>Caller + everyone beneath them.</summary>
        Task<HashSet<int>> GetDownlineScope(int userId);

        /// <summary>Caller + everyone beneath them + their own upline chain.</summary>
        Task<HashSet<int>> GetNetworkScope(int userId);

        /// <summary>True when <paramref name="targetUserId"/> is the caller or in their downline.</summary>
        Task<bool> CanReach(int callerUserId, int callerUserTypeId, int targetUserId);

        /// <summary>Checks every user-reference field in a decrypted request body (JSON).</summary>
        Task<ScopeCheckResult> CheckRequestJson(string controller, string action, int callerUserId, int callerUserTypeId, string? requestJson);

        /// <summary>Checks every user-reference field on a bound DTO, by reflection.</summary>
        Task<ScopeCheckResult> CheckModel(string controller, int callerUserId, int callerUserTypeId, object? model);
    }
}
