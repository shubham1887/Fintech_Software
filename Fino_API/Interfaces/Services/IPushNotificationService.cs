using FintechApp.Requests.PushNotifications;

namespace FintechApp.Interfaces.Services
{
    public interface IPushNotificationService
    {
        Task<string> SendToUserTypes(SendPushNotificationRequest request);
    }
}
