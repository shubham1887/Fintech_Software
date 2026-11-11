using FintechApp.Requests;
using FintechApp.Requests.AppNotification;
using FintechApp.Responses.AppNotification;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The alert feed behind the bell, and the push that mirrors it.
    ///
    /// Every method takes the caller's <see cref="KeyDataModel"/> (the authenticated identity)
    /// rather than a user id off the wire, so nobody can read — or address — somebody else's
    /// notifications.
    /// </summary>
    public interface IAppNotificationService
    {
        Task<(string Status, SendNotificationResult? Result)> Send(SendAppNotificationRequest request, KeyDataModel caller);

        Task<NotificationFeedResponse> GetFeed(GetNotificationFeedRequest request, KeyDataModel caller);

        Task<int> GetUnreadCount(int userId);

        Task<string> MarkRead(MarkNotificationReadRequest request, KeyDataModel caller);

        Task<List<SentNotificationResponse>> GetSent(GetSentNotificationsRequest request);

        Task<string> RegisterDevice(RegisterDeviceRequest request, KeyDataModel caller);

        /// <summary>
        /// Can this installation push to a phone, and is anything registered to push to.
        /// Read by the alert screens so an unconfigured install says so rather than looking as
        /// though it sent something.
        /// </summary>
        Task<PushStatusResponse> GetPushStatus();

        /// <summary>
        /// Raise an alert from inside the platform (a ticket answered, a fund request approved).
        /// Fire-and-forget for the caller: failures are logged, never thrown into a transaction.
        /// </summary>
        Task NotifyUsers(IEnumerable<int> userIds, string title, string body, string kind, string actionUrl);
    }
}
