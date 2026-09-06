using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.RealProviderMapping;
using FintechApp.Responses.RealProviderMapping;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class RealProviderMappingService : IRealProviderMappingService
    {
        private readonly IRealProviderMappingRepository _repository;
        public RealProviderMappingService(IRealProviderMappingRepository repository) { _repository = repository; }

        private static RealProviderMappingResponse MapToResponse(RealProviderMapping e) => new()
        {
            RealProviderMappingID = e.RealProviderMappingID,
            ProviderID = e.ProviderID,
            OperatorID = e.OperatorID,
        };

        public async Task<List<RealProviderMappingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<RealProviderMappingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateRealProviderMappingRequest request)
        {
            var entity = new RealProviderMapping
            {
                ProviderID = request.ProviderID,
                OperatorID = request.OperatorID,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateRealProviderMappingRequest request)
        {
            var entity = await _repository.GetByID(request.RealProviderMappingID);
            if (entity == null) return Messages.RealProviderMappingMessages.NotFound;
            entity.ProviderID = request.ProviderID;
            entity.OperatorID = request.OperatorID;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.RealProviderMappingMessages.NotFound;
        }
    }
}
