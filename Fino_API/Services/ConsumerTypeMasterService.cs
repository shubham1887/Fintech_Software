using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ConsumerTypeMaster;
using FintechApp.Responses.ConsumerTypeMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class ConsumerTypeMasterService : IConsumerTypeMasterService
    {
        private readonly IConsumerTypeMasterRepository _repository;
        public ConsumerTypeMasterService(IConsumerTypeMasterRepository repository) { _repository = repository; }
        private static ConsumerTypeMasterResponse MapToResponse(ConsumerTypeMaster e) => new() { ConsumerTypeMasterID = e.ConsumerTypeMasterID, ConsumerType = e.ConsumerType, };
        public async Task<List<ConsumerTypeMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ConsumerTypeMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateConsumerTypeMasterRequest request) { var entity = new ConsumerTypeMaster { ConsumerType = request.ConsumerType, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateConsumerTypeMasterRequest request) { var entity = await _repository.GetByID(request.ConsumerTypeMasterID); if (entity == null) return Messages.ConsumerTypeMasterMessages.NotFound; entity.ConsumerType = request.ConsumerType; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.ConsumerTypeMasterMessages.NotFound; }
    }
}
