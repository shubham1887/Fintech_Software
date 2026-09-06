using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.WebHookLog;
using FintechApp.Responses.WebHookLog;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class WebHookLogService : IWebHookLogService
    {
        private readonly IWebHookLogRepository _repository;

        public WebHookLogService(IWebHookLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<WebHookLogResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<WebHookLogResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateWebHookLogRequest request)
        {
            var entity = new WebHookLog
            {
                Type = request.Type,
                RequestURL = request.RequestURL,
                Response = request.Response,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateWebHookLogRequest request)
        {
            var entity = await _repository.GetByID(request.WebHookLogID);
            if (entity == null) return WebHookLogMessages.NotFound;
            entity.Type = request.Type;
            entity.RequestURL = request.RequestURL;
            entity.Response = request.Response;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : WebHookLogMessages.NotFound;
        }

        private static WebHookLogResponse MapToResponse(WebHookLog e) => new()
        {
            WebHookLogID = e.WebHookLogID,
            Type = e.Type,
            RequestURL = e.RequestURL,
            Response = e.Response,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
