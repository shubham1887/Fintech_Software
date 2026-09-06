using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.OperatorwiseCallbackSetting;
using FintechApp.Responses.OperatorwiseCallbackSetting;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class OperatorwiseCallbackSettingService : IOperatorwiseCallbackSettingService
    {
        private readonly IOperatorwiseCallbackSettingRepository _repository;

        public OperatorwiseCallbackSettingService(IOperatorwiseCallbackSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<OperatorwiseCallbackSettingResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<OperatorwiseCallbackSettingResponse>> GetByOperatorID(int operatorId)
        {
            var list = await _repository.GetByOperatorID(operatorId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<OperatorwiseCallbackSettingResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateOperatorwiseCallbackSettingRequest request)
        {
            var entity = new OperatorwiseCallbackSetting
            {
                OperatorID = request.OperatorID,
                Url = request.Url,
                MethodType = request.MethodType,
                HeaderJson = request.HeaderJson,
                DateFormat = request.DateFormat,
                IsActive = request.IsActive,
                IsDelete = request.IsDelete,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateOperatorwiseCallbackSettingRequest request)
        {
            var entity = await _repository.GetByID(request.OperatorwiseCallbackSettingID);
            if (entity == null) return OperatorwiseCallbackSettingMessages.NotFound;
            entity.OperatorID = request.OperatorID;
            entity.Url = request.Url;
            entity.MethodType = request.MethodType;
            entity.HeaderJson = request.HeaderJson;
            entity.DateFormat = request.DateFormat;
            entity.IsActive = request.IsActive;
            entity.IsDelete = request.IsDelete;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : OperatorwiseCallbackSettingMessages.NotFound;
        }

        private static OperatorwiseCallbackSettingResponse MapToResponse(OperatorwiseCallbackSetting e) => new()
        {
            OperatorwiseCallbackSettingID = e.OperatorwiseCallbackSettingID,
            OperatorID = e.OperatorID,
            Url = e.Url,
            MethodType = e.MethodType,
            HeaderJson = e.HeaderJson,
            DateFormat = e.DateFormat,
            IsActive = e.IsActive,
            IsDelete = e.IsDelete,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
