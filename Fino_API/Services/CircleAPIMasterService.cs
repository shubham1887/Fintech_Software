using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.CircleAPIMaster;
using FintechApp.Responses.CircleAPIMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class CircleAPIMasterService : ICircleAPIMasterService
    {
        private readonly ICircleAPIMasterRepository _repository;
        public CircleAPIMasterService(ICircleAPIMasterRepository repository) { _repository = repository; }

        private static CircleAPIMasterResponse MapToResponse(CircleAPIMaster e) => new()
        {
            CircleAPIMasterID = e.CircleAPIMasterID,
            CircleAPIURL = e.CircleAPIURL,
            IsActive = e.IsActive,
        };

        public async Task<List<CircleAPIMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<CircleAPIMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateCircleAPIMasterRequest request)
        {
            var entity = new CircleAPIMaster
            {
                CircleAPIURL = request.CircleAPIURL,
                IsActive = request.IsActive,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateCircleAPIMasterRequest request)
        {
            var entity = await _repository.GetByID(request.CircleAPIMasterID);
            if (entity == null) return Messages.CircleAPIMasterMessages.NotFound;
            entity.CircleAPIURL = request.CircleAPIURL;
            entity.IsActive = request.IsActive;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.CircleAPIMasterMessages.NotFound;
        }

        public async Task<string> ActiveDeactive(int id, bool isActive)
        {
            var entity = await _repository.GetByID(id);
            if (entity == null) return Messages.CircleAPIMasterMessages.NotFound;
            entity.IsActive = isActive;
            await _repository.Update(entity);
            return StatusType.Ok;
        }
    }
}
