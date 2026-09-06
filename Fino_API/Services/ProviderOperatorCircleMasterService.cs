using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderOperatorCircleMaster;
using FintechApp.Responses.ProviderOperatorCircleMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class ProviderOperatorCircleMasterService : IProviderOperatorCircleMasterService
    {
        private readonly IProviderOperatorCircleMasterRepository _repository;
        public ProviderOperatorCircleMasterService(IProviderOperatorCircleMasterRepository repository) { _repository = repository; }
        private static ProviderOperatorCircleMasterResponse MapToResponse(ProviderOperatorCircleMaster e) => new() { ProviderOperatorCircleMasterID = e.ProviderOperatorCircleMasterID, ProviderID = e.ProviderID, OperatorID = e.OperatorID, StateID = e.StateID, Opcode = e.Opcode, IsActive = e.IsActive, };
        public async Task<List<ProviderOperatorCircleMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ProviderOperatorCircleMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateProviderOperatorCircleMasterRequest request) { var entity = new ProviderOperatorCircleMaster { ProviderID = request.ProviderID, OperatorID = request.OperatorID, StateID = request.StateID, Opcode = request.Opcode, IsActive = request.IsActive, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateProviderOperatorCircleMasterRequest request) { var entity = await _repository.GetByID(request.ProviderOperatorCircleMasterID); if (entity == null) return Messages.ProviderOperatorCircleMasterMessages.NotFound; entity.ProviderID = request.ProviderID; entity.OperatorID = request.OperatorID; entity.StateID = request.StateID; entity.Opcode = request.Opcode; entity.IsActive = request.IsActive; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.ProviderOperatorCircleMasterMessages.NotFound; }
    }
}
