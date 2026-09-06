using FintechApp.Entities;
using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.StaticModels;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace FintechApp.Services
{
    /// <summary>
    /// Enforces <see cref="HierarchyScopePolicy"/> against the live user tree.
    ///
    /// The tree is UserMaster.ParentID. A caller may name themselves and anyone beneath them;
    /// fields marked <see cref="ScopeKind.Network"/> may additionally name their own upline chain.
    /// Everything else — a peer, another branch, an unrelated distributor's retailer — is denied.
    ///
    /// Mode comes from config "HierarchyScope:Mode": Enforce (default), Audit (log only, useful
    /// for a first rollout) or Off.
    /// </summary>
    public class UserScopeService : IUserScopeService
    {
        private const string ModeEnforce = "Enforce";
        private const string ModeAudit = "Audit";
        private static readonly TimeSpan ScopeTtl = TimeSpan.FromSeconds(60);

        private readonly IUserRepository _userRepository;
        private readonly IMemoryCache _cache;
        private readonly string _mode;
        private readonly Logger _logger = new();

        public UserScopeService(IUserRepository userRepository, IMemoryCache cache, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _cache = cache;
            _mode = configuration["HierarchyScope:Mode"] ?? ModeEnforce;
        }

        public bool IsEnforcing => _mode.Equals(ModeEnforce, StringComparison.OrdinalIgnoreCase);

        private bool IsOff => !IsEnforcing && !_mode.Equals(ModeAudit, StringComparison.OrdinalIgnoreCase);

        public async Task<HashSet<int>> GetDownlineScope(int userId)
        {
            if (_cache.TryGetValue($"scope:down:{userId}", out HashSet<int>? cached) && cached != null) return cached;

            var allUsers = await _userRepository.GetAllUsers();
            var scope = UserHierarchy.GetDownlineIds(allUsers, userId);
            scope.Add(userId); // a caller can always name themselves
            _cache.Set($"scope:down:{userId}", scope, ScopeTtl);
            return scope;
        }

        public async Task<HashSet<int>> GetNetworkScope(int userId)
        {
            if (_cache.TryGetValue($"scope:net:{userId}", out HashSet<int>? cached) && cached != null) return cached;

            var allUsers = await _userRepository.GetAllUsers();
            var scope = UserHierarchy.GetDownlineIds(allUsers, userId);
            scope.Add(userId);
            foreach (var id in UplineIds(allUsers, userId)) scope.Add(id);
            _cache.Set($"scope:net:{userId}", scope, ScopeTtl);
            return scope;
        }

        /// <summary>Walks ParentID upward from <paramref name="userId"/>. Cycle-safe.</summary>
        private static IEnumerable<int> UplineIds(List<UserMaster> allUsers, int userId)
        {
            var byId = allUsers.ToDictionary(u => u.UserMasterID);
            var seen = new HashSet<int> { userId };
            var current = byId.TryGetValue(userId, out var self) ? self.ParentID : null;
            while (current is int parentId && seen.Add(parentId))
            {
                yield return parentId;
                current = byId.TryGetValue(parentId, out var parent) ? parent.ParentID : null;
            }
        }

        public async Task<bool> CanReach(int callerUserId, int callerUserTypeId, int targetUserId)
        {
            if (HierarchyScopePolicy.IsUnrestrictedRole(callerUserTypeId)) return true;
            if (targetUserId <= 0 || targetUserId == callerUserId) return true;
            return (await GetDownlineScope(callerUserId)).Contains(targetUserId);
        }

        public async Task<ScopeCheckResult> CheckRequestJson(string controller, string action, int callerUserId, int callerUserTypeId, string? requestJson)
        {
            if (IsOff) return ScopeCheckResult.Ok;
            if (HierarchyScopePolicy.IsUnrestrictedRole(callerUserTypeId)) return ScopeCheckResult.Ok;
            if (HierarchyScopePolicy.IsExempt(controller, action)) return ScopeCheckResult.Ok;
            if (string.IsNullOrWhiteSpace(requestJson)) return ScopeCheckResult.Ok;

            JToken? parsed;
            try { parsed = JToken.Parse(requestJson); }
            catch { return ScopeCheckResult.Ok; } // not JSON (raw provider payload) — nothing to scope
            return await Evaluate(controller, callerUserId, HierarchyScopePolicy.ScanJson(controller, parsed));
        }

        public async Task<ScopeCheckResult> CheckModel(string controller, int callerUserId, int callerUserTypeId, object? model)
        {
            if (IsOff) return ScopeCheckResult.Ok;
            if (HierarchyScopePolicy.IsUnrestrictedRole(callerUserTypeId)) return ScopeCheckResult.Ok;
            if (model == null) return ScopeCheckResult.Ok;
            return await Evaluate(controller, callerUserId, HierarchyScopePolicy.ScanObject(controller, model));
        }

        private async Task<ScopeCheckResult> Evaluate(string controller, int callerUserId, List<(string Field, ScopeKind Scope, int UserId)> hits)
        {
            if (hits.Count == 0) return ScopeCheckResult.Ok;

            HashSet<int>? downline = null, network = null;
            foreach (var (field, scope, targetId) in hits)
            {
                if (targetId == callerUserId) continue;

                bool ok;
                if (scope == ScopeKind.Network)
                {
                    network ??= await GetNetworkScope(callerUserId);
                    ok = network.Contains(targetId);
                }
                else
                {
                    downline ??= await GetDownlineScope(callerUserId);
                    ok = downline.Contains(targetId);
                }

                if (!ok)
                {
                    var violation = $"{controller}.{field}={targetId} is outside user {callerUserId}'s {scope} scope";
                    _logger.Write(new UnauthorizedAccessException($"Hierarchy scope violation ({_mode}): {violation}"));
                    return _mode.Equals(ModeAudit, StringComparison.OrdinalIgnoreCase)
                        ? ScopeCheckResult.Ok
                        : ScopeCheckResult.Denied(violation);
                }
            }
            return ScopeCheckResult.Ok;
        }
    }
}
