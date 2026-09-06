using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.Notification;
using FintechApp.Responses.Notification;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<NotificationResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<NotificationResponse?> GetByID(int notificationId)
        {
            var entity = await _repository.GetByID(notificationId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateNotificationRequest request)
        {
            var entity = new Notification
            {
                NotificationText = request.NotificationText,
                NotificationImagePath = request.NotificationImagePath,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateNotificationRequest request)
        {
            var entity = await _repository.GetByID(request.NotificationID);
            if (entity == null) return Messages.NotificationMessages.NotFound;

            entity.NotificationText = request.NotificationText;
            entity.NotificationImagePath = request.NotificationImagePath;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int notificationId)
        {
            var entity = await _repository.GetByID(notificationId);
            if (entity == null) return Messages.NotificationMessages.NotFound;

            int result = await _repository.Delete(notificationId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static NotificationResponse MapToResponse(Notification e) => new()
        {
            NotificationID = e.NotificationID,
            NotificationText = e.NotificationText,
            NotificationImagePath = e.NotificationImagePath,
            DateTime = e.DateTime
        };
    }
}
