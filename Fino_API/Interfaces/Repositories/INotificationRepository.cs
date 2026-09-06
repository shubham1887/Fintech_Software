using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetAll();
        Task<Notification?> GetByID(int notificationId);
        Task<int> Insert(Notification entity);
        Task<int> Update(Notification entity);
        Task<int> Delete(int notificationId);
    }
}
