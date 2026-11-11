using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Services;
using FintechApp.Processing;
using FintechApp.Responses.Menu;
using FintechApp.StaticModels;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Services
{
    /// <summary>
    /// The admin's answer to "we do not use that screen".
    ///
    /// Stores exceptions, never the whole menu: a database with no rows here behaves exactly as
    /// the shipped definition does, which is what makes this safe to put on a running install.
    ///
    /// The lookup is cached as one list because <see cref="IsVisible"/> is asked once per menu
    /// node on every sidebar load, and the cache is dropped on every write — an admin who hides a
    /// report and does not see it go would simply hide it again.
    /// </summary>
    public class MenuVisibilityService : IMenuVisibilityService
    {
        private readonly ApplicationDbContext _context;
        private readonly MemoryCacheProcessing<List<MenuVisibilitySetting>> _cache = new();

        public MenuVisibilityService(ApplicationDbContext context) => _context = context;

        private List<MenuVisibilitySetting> All() =>
            _cache.GetOrCreate(CacheKeys.AllMenuVisibility,
                () => _context.MenuVisibilitySettings.AsNoTracking().Where(x => !x.IsDeleted).ToList())
            ?? new List<MenuVisibilitySetting>();

        public Task<bool> IsVisible(string menuKey, int userTypeId)
        {
            var rows = All();
            if (rows.Count == 0) return Task.FromResult(true);

            string key = Normalise(menuKey);

            // Most specific first: a decision made for this role overrides the install-wide one,
            // so "hide it everywhere except admin" is expressible.
            var forRole = rows.FirstOrDefault(r => r.UserTypeID == userTypeId && Normalise(r.MenuKey) == key);
            if (forRole != null) return Task.FromResult(forRole.IsVisible);

            var forAll = rows.FirstOrDefault(r => r.UserTypeID == 0 && Normalise(r.MenuKey) == key);
            if (forAll != null) return Task.FromResult(forAll.IsVisible);

            // No decision: the shipped definition stands.
            return Task.FromResult(true);
        }

        /// <summary>Every key an admin has decided about, as a set — one read for a whole menu build.</summary>
        public Dictionary<(string Key, int Role), bool> Decisions()
        {
            var map = new Dictionary<(string, int), bool>();
            foreach (var r in All()) map[(Normalise(r.MenuKey), r.UserTypeID)] = r.IsVisible;
            return map;
        }

        public Task<List<MenuVisibilityItem>> GetAll() =>
            Task.FromResult(All().Select(r => new MenuVisibilityItem
            {
                MenuKey = r.MenuKey,
                MenuName = r.MenuName,
                UserTypeID = r.UserTypeID,
                IsVisible = r.IsVisible,
                HasOverride = true,
            }).ToList());

        public Task<List<MenuVisibilityItem>> GetCatalog(int userTypeId)
        {
            var decisions = Decisions();
            var items = new List<MenuVisibilityItem>();

            foreach (var entry in MenuService.Catalog())
            {
                string key = Normalise(entry.Key);
                bool hasRole = decisions.TryGetValue((key, userTypeId), out bool roleValue);
                bool hasAll = decisions.TryGetValue((key, 0), out bool allValue);

                items.Add(new MenuVisibilityItem
                {
                    MenuKey = entry.Key,
                    MenuName = entry.Name,
                    GroupName = entry.Group,
                    Url = entry.Url ?? string.Empty,
                    IsGroup = entry.IsGroup,
                    UserTypeID = userTypeId,
                    // What this scope actually resolves to, which is what the admin is editing.
                    IsVisible = hasRole ? roleValue : (hasAll ? allValue : true),
                    // "Somebody chose this" — true only for a decision made at THIS scope, so the
                    // screen does not claim a role-level choice was made when the install-wide
                    // one is doing the work.
                    HasOverride = hasRole || (userTypeId == 0 && hasAll),
                    DefinedForRoles = entry.Roles?.ToList() ?? new List<int>(),
                });
            }

            return Task.FromResult(items);
        }

        public async Task<(string failureMessage, int changed)> Save(List<MenuVisibilityItem> items, int updatedByUserID)
        {
            if (items == null || items.Count == 0) return (Messages.Common.Message.InvalidRequest, 0);

            // Only keys the shipped definition actually has. A key typed by hand would sit in the
            // table for ever hiding nothing, and would be indistinguishable from a renamed screen.
            var known = MenuService.Catalog().Select(c => Normalise(c.Key)).ToHashSet();

            int changed = 0;
            foreach (var item in items)
            {
                string key = Normalise(item.MenuKey);
                if (key.Length == 0 || !known.Contains(key)) continue;
                if (item.UserTypeID < 0) continue;

                var existing = await _context.MenuVisibilitySettings
                    .FirstOrDefaultAsync(x => x.MenuKey == key && x.UserTypeID == item.UserTypeID && !x.IsDeleted);

                // The install-wide scope expresses "visible" by having NO ROW AT ALL, so the table
                // only ever holds real decisions and "no row" keeps meaning "exactly as shipped".
                // That matters beyond tidiness: it is what lets the screen say "As shipped"
                // honestly, and what stops a release that adds a menu entry from being overridden
                // by a stale decision nobody remembers making.
                //
                // A ROLE row saying visible is kept, because it IS a real decision: carving one
                // role out of an install-wide hide can only be said that way.
                bool shouldStore = !item.IsVisible || item.UserTypeID > 0;

                if (existing != null && !shouldStore)
                {
                    // Switched back on for everyone: withdraw the decision rather than storing
                    // "visible". Soft-deleted, so who turned it off and when is still on record,
                    // and the unique index is filtered on IsDeleted so it can be hidden again.
                    existing.IsDeleted = true;
                    existing.UpdatedByUserID = updatedByUserID;
                    existing.UpdatedateTime = DateTime.UtcNow;
                    changed++;
                    continue;
                }

                if (existing == null)
                {
                    if (!shouldStore) continue;
                    _context.MenuVisibilitySettings.Add(new MenuVisibilitySetting
                    {
                        MenuKey = key,
                        MenuName = item.MenuName ?? string.Empty,
                        UserTypeID = item.UserTypeID,
                        IsVisible = item.IsVisible,
                        UpdatedByUserID = updatedByUserID,
                        DateTime = DateTime.UtcNow,
                        UpdatedateTime = DateTime.UtcNow,
                    });
                    changed++;
                }
                else if (existing.IsVisible != item.IsVisible || existing.MenuName != (item.MenuName ?? string.Empty))
                {
                    existing.IsVisible = item.IsVisible;
                    existing.MenuName = item.MenuName ?? string.Empty;
                    existing.UpdatedByUserID = updatedByUserID;
                    existing.UpdatedateTime = DateTime.UtcNow;
                    changed++;
                }
            }

            if (changed > 0)
            {
                await _context.SaveChangesAsync();
                // Drop it now rather than at the 45-minute expiry: an admin who hides a report and
                // does not see it disappear concludes the switch does not work.
                _cache.Remove(CacheKeys.AllMenuVisibility);
            }

            return (StatusType.Ok, changed);
        }

        /// <summary>Keys are matched case-insensitively and without a trailing slash — the same
        /// screen reached as <c>/AEPSReport</c> and <c>/aepsreport</c> is one decision.</summary>
        private static string Normalise(string? key)
        {
            string k = (key ?? string.Empty).Trim().ToLowerInvariant();
            return k.Length > 1 && k.EndsWith('/') ? k[..^1] : k;
        }
    }
}
