using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.WebHookSetting;
using FintechApp.Responses.WebHookSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class WebHookSettingService : IWebHookSettingService
    {
        private readonly IWebHookSettingRepository _repository;

        public WebHookSettingService(IWebHookSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<WebHookSettingResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<WebHookSettingResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateWebHookSettingRequest request)
        {
            var entity = new WebHookSetting
            {
                IsActive = request.IsActive,
                Type = request.Type,
                URL = request.URL,
                MethodType = request.MethodType,
                HeaderJson = request.HeaderJson,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateWebHookSettingRequest request)
        {
            var entity = await _repository.GetByID(request.WebHookSettingID);
            if (entity == null) return WebHookSettingMessages.NotFound;
            entity.IsActive = request.IsActive;
            entity.Type = request.Type;
            entity.URL = request.URL;
            entity.MethodType = request.MethodType;
            entity.HeaderJson = request.HeaderJson;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : WebHookSettingMessages.NotFound;
        }

        private static WebHookSettingResponse MapToResponse(WebHookSetting e) => new()
        {
            WebHookSettingID = e.WebHookSettingID,
            IsActive = e.IsActive,
            Type = e.Type,
            URL = e.URL,
            MethodType = e.MethodType,
            HeaderJson = e.HeaderJson,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
