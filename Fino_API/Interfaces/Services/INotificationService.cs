using FintechApp.Requests.Notification;
using FintechApp.Responses.Notification;

namespace FintechApp.Interfaces.Services
{
    public interface INotificationService
    {
        Task<List<NotificationResponse>> GetAll();
        Task<NotificationResponse?> GetByID(int notificationId);
        Task<string> Create(CreateNotificationRequest request);
        Task<string> Update(UpdateNotificationRequest request);
        Task<string> Delete(int notificationId);
    }
}
