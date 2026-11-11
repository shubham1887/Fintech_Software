using FintechApp.Data;
using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FintechApp.Repositories
{
    /// <summary>
    /// Deliberately uncached. The bell is a per-user, constantly-changing read; the list caches
    /// the master repositories use would serve one retailer another's unread count.
    /// </summary>
    public class AppNotificationRepository : IAppNotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public AppNotificationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------ alerts

        public async Task<int> Insert(AppNotification notification)
        {
            await _context.AppNotifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return notification.AppNotificationID;
        }

        public Task<AppNotification?> GetById(int appNotificationId) =>
            _context.AppNotifications
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.AppNotificationID == appNotificationId && !n.IsDeleted);

        public async Task<int> UpdateCounts(int appNotificationId, int recipientCount, int pushedCount)
        {
            var row = await _context.AppNotifications.FirstOrDefaultAsync(n => n.AppNotificationID == appNotificationId);
            if (row == null) return 0;

            row.RecipientCount = recipientCount;
            row.PushedCount = pushedCount;
            row.UpdatedateTime = DateTime.UtcNow;
            return await _context.SaveChangesAsync();
        }

        public Task<List<AppNotification>> GetSent(DateTime? fromDate, DateTime? toDate, int skip, int take)
        {
            var query = _context.AppNotifications.AsNoTracking().Where(n => !n.IsDeleted);

            if (fromDate.HasValue) query = query.Where(n => n.DateTime >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(n => n.DateTime <= toDate.Value);

            return query.OrderByDescending(n => n.AppNotificationID).Skip(skip).Take(take).ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetReadCounts(IEnumerable<int> appNotificationIds)
        {
            var ids = appNotificationIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<int, int>();

            var rows = await _context.AppNotificationRecipients
                .AsNoTracking()
                .Where(r => ids.Contains(r.AppNotificationID) && r.IsRead)
                .GroupBy(r => r.AppNotificationID)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync();

            return rows.ToDictionary(r => r.Key, r => r.Count);
        }

        // -------------------------------------------------------------- recipients

        public async Task<int> InsertRecipients(List<AppNotificationRecipient> recipients)
        {
            if (recipients.Count == 0) return 0;
            await _context.AppNotificationRecipients.AddRangeAsync(recipients);
            return await _context.SaveChangesAsync();
        }

        public async Task<(List<AppNotificationRecipient> Items, int Total, int Unread)> GetFeed(
            int userId, bool unreadOnly, string kind, int skip, int take)
        {
            var mine = _context.AppNotificationRecipients
                .AsNoTracking()
                .Include(r => r.AppNotification)
                .Where(r => r.UserID == userId && !r.IsDeleted && r.AppNotification != null && !r.AppNotification.IsDeleted)
                // A retired alert leaves the bell — including the unread badge, which is counted
                // off this same query so the number can never disagree with the list under it.
                .Where(r => r.AppNotification!.ExpiresOn == null || r.AppNotification.ExpiresOn > DateTime.Now);

            if (!string.IsNullOrWhiteSpace(kind))
                mine = mine.Where(r => r.AppNotification!.Kind == kind);

            // Counted before paging: the badge is "how many unread in total", not "on this page".
            int total = await mine.CountAsync();
            int unread = await mine.CountAsync(r => !r.IsRead);

            if (unreadOnly) mine = mine.Where(r => !r.IsRead);

            var items = await mine
                .OrderByDescending(r => r.AppNotificationRecipientID)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (items, total, unread);
        }

        public async Task<int> MarkRead(int userId, int appNotificationId)
        {
            var rows = await _context.AppNotificationRecipients
                .Where(r => r.UserID == userId && r.AppNotificationID == appNotificationId && !r.IsRead)
                .ToListAsync();

            foreach (var row in rows)
            {
                row.IsRead = true;
                row.ReadDateTime = DateTime.UtcNow;
                row.UpdatedateTime = DateTime.UtcNow;
            }

            return rows.Count == 0 ? 0 : await _context.SaveChangesAsync();
        }

        public async Task<int> MarkAllRead(int userId)
        {
            var rows = await _context.AppNotificationRecipients
                .Where(r => r.UserID == userId && !r.IsRead)
                .ToListAsync();

            foreach (var row in rows)
            {
                row.IsRead = true;
                row.ReadDateTime = DateTime.UtcNow;
                row.UpdatedateTime = DateTime.UtcNow;
            }

            return rows.Count == 0 ? 0 : await _context.SaveChangesAsync();
        }

        public async Task<int> MarkPushed(int appNotificationId, IEnumerable<int> userIds)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0) return 0;

            var rows = await _context.AppNotificationRecipients
                .Where(r => r.AppNotificationID == appNotificationId && ids.Contains(r.UserID))
                .ToListAsync();

            foreach (var row in rows)
            {
                row.PushSent = true;
                row.PushDateTime = DateTime.UtcNow;
            }

            return rows.Count == 0 ? 0 : await _context.SaveChangesAsync();
        }

        // ----------------------------------------------------------------- devices

        public Task<UserDevice?> GetDeviceByToken(string fcmToken) =>
            _context.UserDevices.FirstOrDefaultAsync(d => d.FcmToken == fcmToken);

        public Task<List<UserDevice>> GetActiveDevices(IEnumerable<int> userIds)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0) return Task.FromResult(new List<UserDevice>());

            return _context.UserDevices
                .AsNoTracking()
                .Where(d => ids.Contains(d.UserID) && d.IsActive && !d.IsDeleted)
                .ToListAsync();
        }

        public Task<int> CountActiveDevices(string platform)
        {
            var query = _context.UserDevices.AsNoTracking().Where(d => d.IsActive && !d.IsDeleted);
            // Blank platform = every kind of device. Platform is stored lower-cased by
            // RegisterDevice, so the comparison needs no normalising here.
            if (!string.IsNullOrWhiteSpace(platform))
            {
                string wanted = platform.Trim().ToLowerInvariant();
                query = query.Where(d => d.Platform == wanted);
            }
            return query.CountAsync();
        }

        /// <summary>
        /// One row per token. The same handset signing in as a different user moves the row
        /// rather than duplicating it — otherwise the previous user would keep getting alerts
        /// on a phone that is no longer theirs.
        /// </summary>
        public async Task<int> UpsertDevice(UserDevice device)
        {
            var existing = await _context.UserDevices.FirstOrDefaultAsync(d => d.FcmToken == device.FcmToken);

            if (existing == null)
            {
                await _context.UserDevices.AddAsync(device);
                return await _context.SaveChangesAsync();
            }

            existing.UserID = device.UserID;
            existing.Platform = device.Platform;
            existing.DeviceName = device.DeviceName;
            existing.AppVersion = device.AppVersion;
            existing.IsActive = true;
            existing.IsDeleted = false;
            existing.DeactivatedReason = string.Empty;
            existing.LastSeenDateTime = DateTime.UtcNow;
            existing.UpdatedateTime = DateTime.UtcNow;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeactivateDevice(string fcmToken, string reason)
        {
            var row = await _context.UserDevices.FirstOrDefaultAsync(d => d.FcmToken == fcmToken);
            if (row == null) return 0;

            row.IsActive = false;
            row.DeactivatedReason = reason;
            row.UpdatedateTime = DateTime.UtcNow;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeactivateDevices(IEnumerable<string> fcmTokens, string reason)
        {
            var tokens = fcmTokens.Distinct().ToList();
            if (tokens.Count == 0) return 0;

            var rows = await _context.UserDevices.Where(d => tokens.Contains(d.FcmToken)).ToListAsync();
            foreach (var row in rows)
            {
                row.IsActive = false;
                row.DeactivatedReason = reason;
                row.UpdatedateTime = DateTime.UtcNow;
            }

            return rows.Count == 0 ? 0 : await _context.SaveChangesAsync();
        }
    }
}
