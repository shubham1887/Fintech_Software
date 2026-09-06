using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.EmailSetting;
using FintechApp.Responses.EmailSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class EmailSettingService : IEmailSettingService
    {
        private readonly IEmailSettingRepository _repository;

        public EmailSettingService(IEmailSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<EmailSettingResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<EmailSettingResponse?> GetByID(int emailSettingId)
        {
            var entity = await _repository.GetByID(emailSettingId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateEmailSettingRequest request)
        {
            var entity = new EmailSettingMaster
            {
                Email = request.Email,
                Password = request.Password,
                HostName = request.HostName,
                Port = request.Port,
                IsSSL = request.IsSSL,
                IsActive = request.IsActive,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateEmailSettingRequest request)
        {
            var entity = await _repository.GetByID(request.EmailSettingMasterID);
            if (entity == null) return Messages.EmailSettingMessages.NotFound;

            entity.Email = request.Email;
            entity.Password = request.Password;
            entity.HostName = request.HostName;
            entity.Port = request.Port;
            entity.IsSSL = request.IsSSL;
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int emailSettingId)
        {
            var entity = await _repository.GetByID(emailSettingId);
            if (entity == null) return Messages.EmailSettingMessages.NotFound;

            int result = await _repository.Delete(emailSettingId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static EmailSettingResponse MapToResponse(EmailSettingMaster e) => new()
        {
            EmailSettingMasterID = e.EmailSettingMasterID,
            Email = e.Email,
            HostName = e.HostName,
            Port = e.Port,
            IsSSL = e.IsSSL,
            IsActive = e.IsActive,
            DateTime = e.DateTime
        };
    }
}
