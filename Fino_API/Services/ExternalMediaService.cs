using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ExternalMedia;
using FintechApp.Responses.ExternalMedia;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ExternalMediaService : IExternalMediaService
    {
        private readonly IExternalMediaRepository _repository;
        public ExternalMediaService(IExternalMediaRepository repository) { _repository = repository; }

        private static ExternalMediaResponse MapToResponse(ExternalMedia e) => new()
        {
            ExternalMediaID = e.ExternalMediaID,
            Key = e.Key,
            Value = e.Value,
            Description = e.Description,
            IsActive = e.IsActive,
        };

        public async Task<List<ExternalMediaResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ExternalMediaResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateExternalMediaRequest request)
        {
            var entity = new ExternalMedia
            {
                Key = request.Key,
                Value = request.Value,
                Description = request.Description,
                IsActive = request.IsActive,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateExternalMediaRequest request)
        {
            var entity = await _repository.GetByID(request.ExternalMediaID);
            if (entity == null) return Messages.ExternalMediaMessages.NotFound;
            entity.Key = request.Key;
            entity.Value = request.Value;
            entity.Description = request.Description;
            entity.IsActive = request.IsActive;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ExternalMediaMessages.NotFound;
        }
    }
}
