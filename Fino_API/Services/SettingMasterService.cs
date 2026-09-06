using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SettingMaster;
using FintechApp.Responses.SettingMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class SettingMasterService : ISettingMasterService
    {
        private readonly ISettingMasterRepository _repository;

        public SettingMasterService(ISettingMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SettingMasterResponse>> GetAll()
        {
            var all = await _repository.GetAll();
            return all.Select(MapToResponse).ToList();
        }

        public async Task<SettingMasterResponse?> GetByID(int settingMasterId)
        {
            var entity = await _repository.GetByID(settingMasterId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateSettingMasterRequest request)
        {
            var entity = new SettingMaster
            {
                Key = request.Key,
                Value = request.Value,
                IsOn = request.IsOn,
                IsSecure = request.IsSecure,
                Description = request.Description,
                IsPassword = request.IsPassword,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow
            };

            int result = await _repository.Insert(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Update(UpdateSettingMasterRequest request)
        {
            var entity = await _repository.GetByID(request.SettingMasterID);
            if (entity == null) return Messages.SettingMasterMessages.NotFound;

            entity.Key = request.Key;
            entity.Value = request.Value;
            entity.IsOn = request.IsOn;
            entity.IsSecure = request.IsSecure;
            entity.Description = request.Description;
            entity.IsPassword = request.IsPassword;
            entity.UpdatedateTime = DateTime.UtcNow;

            int result = await _repository.Update(entity);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        public async Task<string> Delete(int settingMasterId)
        {
            var entity = await _repository.GetByID(settingMasterId);
            if (entity == null) return Messages.SettingMasterMessages.NotFound;

            int result = await _repository.Delete(settingMasterId);
            return result > 0 ? StatusType.Ok : Messages.Common.Message.Failed;
        }

        private static SettingMasterResponse MapToResponse(SettingMaster e) => new()
        {
            SettingMasterID = e.SettingMasterID,
            Key = e.Key,
            Value = e.Value,
            IsOn = e.IsOn,
            IsSecure = e.IsSecure,
            Description = e.Description,
            IsPassword = e.IsPassword,
            DateTime = e.DateTime
        };
    }
}
