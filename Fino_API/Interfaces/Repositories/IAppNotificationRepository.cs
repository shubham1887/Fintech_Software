using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    /// <summary>Storage for the alert feed and the devices a push can reach.</summary>
    public interface IAppNotificationRepository
    {
        // ---- alerts ----
        Task<int> Insert(AppNotification notification);
        Task<AppNotification?> GetById(int appNotificationId);
        Task<int> UpdateCounts(int appNotificationId, int recipientCount, int pushedCount);
        Task<List<AppNotification>> GetSent(DateTime? fromDate, DateTime? toDate, int skip, int take);

        /// <summary>Read counts per alert id — for the admin's sent list.</summary>
        Task<Dictionary<int, int>> GetReadCounts(IEnumerable<int> appNotificationIds);

        // ---- recipients ----
        Task<int> InsertRecipients(List<AppNotificationRecipient> recipients);
        Task<(List<AppNotificationRecipient> Items, int Total, int Unread)> GetFeed(
            int userId, bool unreadOnly, string kind, int skip, int take);
        Task<int> MarkRead(int userId, int appNotificationId);
        Task<int> MarkAllRead(int userId);
        Task<int> MarkPushed(int appNotificationId, IEnumerable<int> userIds);

        // ---- devices ----
        Task<UserDevice?> GetDeviceByToken(string fcmToken);
        Task<List<UserDevice>> GetActiveDevices(IEnumerable<int> userIds);

        /// <summary>
        /// How many handsets could receive a push right now, platform-wide. Counts DEVICES, not
        /// users — one retailer with a phone and a browser is two addresses — and is used to
        /// answer "is anything registered at all?" when alerts are not arriving.
        /// </summary>
        Task<int> CountActiveDevices(string platform);
        Task<int> UpsertDevice(UserDevice device);
        Task<int> DeactivateDevice(string fcmToken, string reason);
        Task<int> DeactivateDevices(IEnumerable<string> fcmTokens, string reason);
    }
}
