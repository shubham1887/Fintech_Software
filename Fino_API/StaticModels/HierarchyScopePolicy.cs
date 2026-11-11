using System.Collections.Concurrent;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace FintechApp.StaticModels
{
    /// <summary>How far a user-reference field is allowed to point.</summary>
    public enum ScopeKind
    {
        /// <summary>Not a user reference — ignored.</summary>
        None = 0,
        /// <summary>The caller themselves or anyone BELOW them in the tree (children, grandchildren…).</summary>
        Downline = 1,
        /// <summary>The caller's own branch — self + downline + their own upline chain (parent,
        /// grandparent…). For fields that legitimately point upward (a fund request goes UP to the
        /// parent). Peers and other branches are still blocked, which is the actual leak.</summary>
        Network = 2,
    }

    /// <summary>
    /// Central, deny-by-default map of "which request fields carry a user id, and how far may that
    /// id point". Companion to <see cref="ApiAccessPolicy"/>: ApiAccessPolicy answers *which
    /// endpoint* a role may call, this answers *whose rows* it may name once inside.
    ///
    /// Enforcement is generic — <see cref="ScanJson"/> walks the decrypted request body and
    /// <see cref="ScanObject"/> walks a bound DTO by REFLECTION — so a new controller/request is
    /// covered the moment it reuses the standard field names. Nothing per-controller to add.
    /// </summary>
    public static class HierarchyScopePolicy
    {
        // Roles that operate the whole network and are therefore unscoped. Mirrors the `isAdmin`
        // test already used in UserService.Select.
        public static bool IsUnrestrictedRole(int userTypeId) =>
            userTypeId == UserTypeCode.Admin || userTypeId == UserTypeCode.SupportUser;

        // Field names that mean "a UserMaster id" in EVERY controller. Unambiguous by name.
        private static readonly Dictionary<string, ScopeKind> GlobalFields = new(StringComparer.OrdinalIgnoreCase)
        {
            ["UserID"] = ScopeKind.Downline,
            ["UserMasterID"] = ScopeKind.Downline,
            ["TargetUserID"] = ScopeKind.Downline,
            ["SelectedUserID"] = ScopeKind.Downline,
            ["AssignedUserID"] = ScopeKind.Downline,
            ["DownlineUserID"] = ScopeKind.Downline,
            ["RetailerID"] = ScopeKind.Downline,
            ["ToUserID"] = ScopeKind.Downline,

            // Legitimately upward-pointing references.
            ["ParentID"] = ScopeKind.Network,
            // The destination of a re-parent (User Transfer, Userwise Bulk Action "Shift"). It was
            // missing, so this generic gate saw nothing to check on the one payload whose whole
            // purpose is to move an account somewhere else in the tree. Both services validate it
            // precisely as well; this is the net underneath them, and it covers any future
            // endpoint that reuses the name.
            ["NewParentID"] = ScopeKind.Network,
            ["UplineUserID"] = ScopeKind.Network,
            ["FromUserID"] = ScopeKind.Network,
            ["DistributerUserID"] = ScopeKind.Network,
            ["DistributorUserID"] = ScopeKind.Network,
            ["SuperDistributerUserID"] = ScopeKind.Network,
            ["SuperDistributorUserID"] = ScopeKind.Network,
        };

        // Field names that are a UserMaster id ONLY inside certain controllers. "SenderID", for
        // example, is a user in Fund* but a DMTSender row id in DMT / Beneficiary — scoping it
        // globally would 403 legitimate money-transfer traffic.
        // Key format: "controller-lower.field-lower".
        private static readonly Dictionary<string, ScopeKind> ControllerFields = new(StringComparer.OrdinalIgnoreCase)
        {
            ["fund.senderid"] = ScopeKind.Downline,      // reverse: pull funds back from a downline
            ["fund.receiverid"] = ScopeKind.Downline,    // transfer: push funds down
            ["fund.requestto"] = ScopeKind.Network,      // fund request goes UP to the parent
            ["fund.requesttoid"] = ScopeKind.Network,
            ["fundtransaction.senderid"] = ScopeKind.Network,
            ["fundtransaction.receiverid"] = ScopeKind.Network,
            ["fundtransaction.requesttoid"] = ScopeKind.Network,
            ["wallettowallettransfer.receiverid"] = ScopeKind.Downline,
            ["wallettowallettransfer.senderid"] = ScopeKind.Network,
            ["cashcollection.receiverid"] = ScopeKind.Downline,
            ["cashcollectiondetail.receiverid"] = ScopeKind.Downline,
        };

        // Controllers whose request bodies never name another user in the hierarchy sense
        // (auth / public / provider callbacks). Scanning them would only risk false denials.
        private static readonly HashSet<string> ExemptControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "auth", "login", "outsideregistration", "registration", "signupekyc", "publicapi",
            "callback", "ecollectioncallback", "onboardingcallback", "payswiffcallback",
            "apirecharge", "pingeneration", "publicslidermaster", "forgotpassword",
        };

        // Exact "controller.action" escapes, for the rare endpoint that must accept an
        // out-of-branch id. Kept explicit so every exception is visible in review.
        private static readonly HashSet<string> ExemptActions = new(StringComparer.OrdinalIgnoreCase)
        {
            "users.checkavailability",
        };

        public static bool IsExempt(string controller, string action)
        {
            var c = Normalize(controller);
            return ExemptControllers.Contains(c) || ExemptActions.Contains($"{c}.{action.ToLowerInvariant()}");
        }

        /// <summary>Scope for <paramref name="field"/> in <paramref name="controller"/>, or None.</summary>
        public static ScopeKind ScopeFor(string controller, string field)
        {
            var c = Normalize(controller);
            if (ControllerFields.TryGetValue($"{c}.{field.ToLowerInvariant()}", out var byController)) return byController;
            return GlobalFields.TryGetValue(field, out var global) ? global : ScopeKind.None;
        }

        /// <summary>Trims a trailing "Controller" and lowercases — same convention as ApiAccessPolicy.</summary>
        public static string Normalize(string controller)
        {
            var c = controller ?? string.Empty;
            if (c.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
                c = c[..^"Controller".Length];
            return c.ToLowerInvariant();
        }

        // ── JSON path (the request body as it arrives, before any controller sees it) ────

        /// <summary>
        /// Every (field, scope, userId) reference in a decrypted request body. Walks nested objects
        /// and arrays, so bulk/batch payloads are covered too. Zero / negative / unparseable values
        /// are skipped — they mean "not supplied".
        /// </summary>
        public static List<(string Field, ScopeKind Scope, int UserId)> ScanJson(string controller, JToken? token)
        {
            var hits = new List<(string, ScopeKind, int)>();
            if (token != null) Walk(controller, token, hits, 0);
            return hits;
        }

        private static void Walk(string controller, JToken token, List<(string, ScopeKind, int)> hits, int depth)
        {
            if (depth > 8) return; // pathological nesting guard
            switch (token)
            {
                case JObject obj:
                    foreach (var prop in obj.Properties())
                    {
                        var scope = ScopeFor(controller, prop.Name);
                        if (scope != ScopeKind.None && TryUserId(prop.Value, out int id))
                            hits.Add((prop.Name, scope, id));
                        else
                            Walk(controller, prop.Value, hits, depth + 1);
                    }
                    break;
                case JArray arr:
                    foreach (var item in arr) Walk(controller, item, hits, depth + 1);
                    break;
            }
        }

        private static bool TryUserId(JToken value, out int id)
        {
            id = 0;
            if (value.Type is JTokenType.Integer or JTokenType.Float)
            {
                id = value.Value<int>();
                return id > 0;
            }
            if (value.Type == JTokenType.String)
                return int.TryParse(value.Value<string>(), out id) && id > 0;
            return false;
        }

        // ── Reflection path (for a service holding an already-bound DTO) ─────────────────
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

        /// <summary>
        /// Same rules as <see cref="ScanJson"/> but over a CLR object, by reflection. Lets a service
        /// re-validate a DTO it holds (defence in depth) without enumerating fields by hand.
        /// </summary>
        public static List<(string Field, ScopeKind Scope, int UserId)> ScanObject(string controller, object? model)
        {
            var hits = new List<(string, ScopeKind, int)>();
            if (model != null)
                WalkObject(controller, model, hits, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
            return hits;
        }

        private static void WalkObject(string controller, object model, List<(string, ScopeKind, int)> hits, int depth, HashSet<object> seen)
        {
            if (depth > 6 || !seen.Add(model)) return;

            if (model is System.Collections.IEnumerable seq and not string)
            {
                foreach (var item in seq)
                    if (item != null && !IsScalar(item.GetType())) WalkObject(controller, item, hits, depth + 1, seen);
                return;
            }

            var props = PropertyCache.GetOrAdd(model.GetType(), t => t
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .ToArray());

            foreach (var prop in props)
            {
                object? value;
                try { value = prop.GetValue(model); } catch { continue; }
                if (value == null) continue;

                var scope = ScopeFor(controller, prop.Name);
                if (scope != ScopeKind.None)
                {
                    if (value is int i && i > 0) hits.Add((prop.Name, scope, i));
                    else if (value is long l && l > 0 && l <= int.MaxValue) hits.Add((prop.Name, scope, (int)l));
                    else if (value is string s && int.TryParse(s, out int parsed) && parsed > 0) hits.Add((prop.Name, scope, parsed));
                    continue;
                }

                if (!IsScalar(prop.PropertyType)) WalkObject(controller, value, hits, depth + 1, seen);
            }
        }

        private static bool IsScalar(Type t)
        {
            var u = Nullable.GetUnderlyingType(t) ?? t;
            return u.IsPrimitive || u.IsEnum || u == typeof(string) || u == typeof(decimal)
                || u == typeof(DateTime) || u == typeof(DateTimeOffset) || u == typeof(Guid) || u == typeof(TimeSpan);
        }
    }
}
