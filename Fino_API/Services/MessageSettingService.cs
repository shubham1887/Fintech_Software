using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.MessageSetting;
using FintechApp.Responses.MessageSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class MessageSettingService : IMessageSettingService
    {
        private readonly IMessageSettingRepository _repository;

        public MessageSettingService(IMessageSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MessageSettingResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<MessageSettingResponse?> GetByID(int messageSettingId)
        {
            var entity = await _repository.GetByID(messageSettingId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateMessageSettingRequest request)
        {
            var entity = new MessageSetting
            {
                Type = request.Type,
                IsSMS = request.IsSMS,
                IsMail = request.IsMail,
                IsWhatsUp = request.IsWhatsUp,
                WhatsUpMessage = request.WhatsUpMessage,
                SmsMessage = request.SmsMessage,
                EmailMessage = request.EmailMessage,
                TemplateID = request.TemplateID,
                IsOTP = request.IsOTP,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };
            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateMessageSettingRequest request)
        {
            var entity = await _repository.GetByID(request.MessageSettingID);
            if (entity == null) return Messages.MessageSettingMessages.NotFound;

            entity.Type = request.Type;
            entity.IsSMS = request.IsSMS;
            entity.IsMail = request.IsMail;
            entity.IsWhatsUp = request.IsWhatsUp;
            entity.WhatsUpMessage = request.WhatsUpMessage;
            entity.SmsMessage = request.SmsMessage;
            entity.EmailMessage = request.EmailMessage;
            entity.TemplateID = request.TemplateID;
            entity.IsOTP = request.IsOTP;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int messageSettingId)
        {
            var entity = await _repository.GetByID(messageSettingId);
            if (entity == null) return Messages.MessageSettingMessages.NotFound;
            int result = await _repository.Delete(messageSettingId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static MessageSettingResponse MapToResponse(MessageSetting e) => new()
        {
            MessageSettingID = e.MessageSettingID,
            Type = e.Type,
            IsSMS = e.IsSMS,
            IsMail = e.IsMail,
            IsWhatsUp = e.IsWhatsUp,
            WhatsUpMessage = e.WhatsUpMessage,
            SmsMessage = e.SmsMessage,
            EmailMessage = e.EmailMessage,
            TemplateID = e.TemplateID,
            IsOTP = e.IsOTP,
            DateTime = e.DateTime
        };
    }
}
