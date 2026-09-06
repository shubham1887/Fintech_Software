namespace FintechApp.Interfaces.Services
{
    /// <summary>Resolves and enforces which user-types may call a given controller/action.</summary>
    public interface IApiAccessPolicyService
    {
        /// <summary>True if the role may call this controller/action (deny-by-default).</summary>
        bool IsAllowed(string controller, string action, int roleId);

        /// <summary>Allowed role ids for a controller/action (defaults + deployment overrides).</summary>
        int[] AllowedRoles(string controller, string action);

        /// <summary>Re-read deployment overrides from configuration (hot-reload).</summary>
        void Reload();
    }
}
