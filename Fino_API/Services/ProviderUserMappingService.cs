using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.ProviderUserMapping;
using FintechApp.Responses.ProviderUserMapping;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderUserMappingService : IProviderUserMappingService
    {
        private readonly IProviderUserMappingRepository _repository;

        public ProviderUserMappingService(IProviderUserMappingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProviderUserMappingResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<ProviderUserMappingResponse>> GetByUserID(int userId)
        {
            var list = await _repository.GetByUserID(userId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<ProviderUserMappingResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateProviderUserMappingRequest request)
        {
            var entity = new ProviderUserMapping
            {
                Key = request.Key,
                Value = request.Value,
                ProviderID = request.ProviderID,
                UserID = request.UserID,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateProviderUserMappingRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderUserMappingID);
            if (entity == null) return ProviderUserMappingMessages.NotFound;
            entity.Key = request.Key;
            entity.Value = request.Value;
            entity.ProviderID = request.ProviderID;
            entity.UserID = request.UserID;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : ProviderUserMappingMessages.NotFound;
        }

        private static ProviderUserMappingResponse MapToResponse(ProviderUserMapping e) => new()
        {
            ProviderUserMappingID = e.ProviderUserMappingID,
            Key = e.Key,
            Value = e.Value,
            ProviderID = e.ProviderID,
            UserID = e.UserID,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
