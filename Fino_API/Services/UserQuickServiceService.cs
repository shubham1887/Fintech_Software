using FintechApp.Helpers;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.QuickService;
using FintechApp.Responses.QuickService;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    /// <summary>
    /// "Quick Services" — the shortcut tiles a user pins for themselves.
    ///
    /// The admin owns the catalogue and its order; this owns one user's selection out of it. Both
    /// reads and writes are checked against the catalogue the caller is actually entitled to, so
    /// a shortcut can never become a back door to a service that was switched off for them, and a
    /// stale pin left over from before a change simply stops being returned.
    /// </summary>
    public class UserQuickServiceService : IUserQuickServiceService
    {
        private readonly IUserQuickServiceRepository _quickRepo;
        private readonly IServiceService _serviceService;
        private readonly Logger _logger;

        /// <summary>
        /// A shortcut row is a shortcut, not a second menu — past about eight tiles it stops
        /// saving anyone time and starts being the grid it was meant to shorten.
        /// </summary>
        private const int MaxShortcuts = 8;

        public UserQuickServiceService(IUserQuickServiceRepository quickRepo, IServiceService serviceService)
        {
            _quickRepo = quickRepo;
            _serviceService = serviceService;
            _logger = new Logger();
        }

        /// <summary>
        /// Quick Services belong to the retailer and to nobody else.
        ///
        /// It is the counter operator's shortcut row — the one role that opens the same handful of
        /// services all day, where hunting through the grid costs real time. Every other role
        /// either does not transact at all (admin, support, accountant, field officers), works from
        /// reports and network screens rather than a service grid (distributor tier), or has no
        /// dashboard whatsoever: an API user is a server-to-server credential that reaches the
        /// platform over HTTP and has no tile grid to shorten.
        ///
        /// <see cref="StaticModels.ApiAccessPolicy"/> and the panel both hide it for everyone else;
        /// this is the service agreeing with them rather than relying on them. Pins already saved
        /// by another role stay in the table but are never served — nothing is deleted on a role
        /// change, so a user moved to retailer and back finds their row intact.
        /// </summary>
        private static bool HasQuickServices(int userTypeId) => userTypeId == UserTypeCode.Retailer;

        public async Task<(string failureMessage, List<QuickServiceViewModel> data)> Select(int userId, int userTypeId)
        {
            var result = new List<QuickServiceViewModel>();
            if (!HasQuickServices(userTypeId)) return (StatusType.Ok, result);
            try
            {
                var pinned = await _quickRepo.GetByUserID(userId);
                if (pinned.Count == 0) return (StatusType.Ok, result);

                var allowed = await AllowedServices(userId, userTypeId);

                foreach (var pin in pinned.OrderBy(p => p.UISequence))
                {
                    if (!allowed.TryGetValue(pin.ServiceID, out var entry)) continue;
                    var service = entry.Service;
                    result.Add(new QuickServiceViewModel
                    {
                        ServiceID = service.ServiceID,
                        Name = service.Name,
                        ServiceCategory = entry.Category,
                        ServiceImagePath = service.ServiceImagePath,
                        UISequence = pin.UISequence,
                        IsLocked = service.IsLocked,
                        LockReason = service.LockReason,
                    });
                }

                return (StatusType.Ok, result);
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return (Messages.Common.Message.ExceptionOccured, result);
            }
        }

        public async Task<string> Save(int userId, int userTypeId, SaveQuickServiceRequest request)
        {
            if (!HasQuickServices(userTypeId))
                return "Quick Services are available to retailer accounts only.";
            try
            {
                var ids = (request.ServiceIDs ?? new List<int>()).Where(id => id > 0).Distinct().ToList();
                if (ids.Count > MaxShortcuts)
                    return $"You can pin up to {MaxShortcuts} quick services.";

                // Only services this user may actually open. Without this a client could pin — and
                // then be shown a tile for — a service the admin has switched off for them.
                var allowed = await AllowedServices(userId, userTypeId);
                var unknown = ids.Where(id => !allowed.ContainsKey(id)).ToList();
                if (unknown.Count > 0)
                    return "One of the selected services is not available on your account.";

                await _quickRepo.ReplaceForUser(userId, ids);
                return StatusType.Ok;
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
                return Messages.Common.Message.ExceptionOccured;
            }
        }

        /// <summary>
        /// The services this caller may pin, keyed by id — the same catalogue their dashboard
        /// renders, so the picker and the grid can never disagree about what exists. Admins get
        /// the full catalogue (they see hidden/inactive rows everywhere else too).
        /// </summary>
        private async Task<Dictionary<int, (string Category, Responses.Service.CatalogServiceItem Service)>> AllowedServices(
            int userId, int userTypeId)
        {
            bool includeHidden = userTypeId == UserTypeCode.Admin;
            var catalog = await _serviceService.GetServiceCatalog(includeHidden, userId);
            return catalog
                .SelectMany(c => c.Services.Select(s => (Category: c.Category, Service: s)))
                .GroupBy(x => x.Service.ServiceID)
                .ToDictionary(g => g.Key, g => g.First());
        }
    }
}
