using System.Collections.Concurrent;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// Enforces the deny-by-default <see cref="ApiAccessPolicy"/>, layered with per-deployment
    /// overrides from configuration section "ApiAccess:Overrides" (so a reseller can re-scope
    /// access per client without a code change). Keys are "controller" or "controller.action";
    /// values are role-id lists (CSV string or array). Admin (UserTypeCode.Admin) is always
    /// allowed as a safety net against accidental lock-out.
    /// </summary>
    public class ApiAccessPolicyService : IApiAccessPolicyService
    {
        private readonly IConfiguration _config;
        private volatile ConcurrentDictionary<string, int[]> _overrides;

        public ApiAccessPolicyService(IConfiguration config)
        {
            _config = config;
            _overrides = LoadOverrides(config);
        }

        public int[] AllowedRoles(string controller, string action)
        {
            var c = ApiAccessPolicy.Normalize(controller);
            var a = action.ToLowerInvariant();
            if (_overrides.TryGetValue($"{c}.{a}", out var byAction)) return byAction;
            if (_overrides.TryGetValue(c, out var byController)) return byController;
            return ApiAccessPolicy.Resolve(controller, action);
        }

        public bool IsAllowed(string controller, string action, int roleId)
        {
            if (roleId == UserTypeCode.Admin) return true; // superadmin safety net
            return Array.IndexOf(AllowedRoles(controller, action), roleId) >= 0;
        }

        public void Reload() => _overrides = LoadOverrides(_config);

        private static ConcurrentDictionary<string, int[]> LoadOverrides(IConfiguration config)
        {
            var map = new ConcurrentDictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
            var section = config.GetSection("ApiAccess:Overrides");
            foreach (var child in section.GetChildren())
            {
                int[] roles;
                if (child.Value != null)
                {
                    roles = child.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(s => int.TryParse(s, out var v) ? v : -1).Where(v => v >= 0).ToArray();
                }
                else
                {
                    roles = child.GetChildren()
                        .Select(x => int.TryParse(x.Value, out var v) ? v : -1).Where(v => v >= 0).ToArray();
                }
                if (roles.Length > 0) map[child.Key.ToLowerInvariant()] = roles;
            }
            return map;
        }
    }
}
