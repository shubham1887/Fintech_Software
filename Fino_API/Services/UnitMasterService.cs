using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UnitMaster;
using FintechApp.Responses.UnitMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UnitMasterService : IUnitMasterService
    {
        private readonly IUnitMasterRepository _repository;
        public UnitMasterService(IUnitMasterRepository repository) { _repository = repository; }
        private static UnitMasterResponse MapToResponse(UnitMaster e) => new() { UnitMasterID = e.UnitMasterID, Name = e.Name, };
        public async Task<List<UnitMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UnitMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateUnitMasterRequest request) { var entity = new UnitMaster { Name = request.Name, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUnitMasterRequest request) { var entity = await _repository.GetByID(request.UnitMasterID); if (entity == null) return Messages.UnitMasterMessages.NotFound; entity.Name = request.Name; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UnitMasterMessages.NotFound; }
    }
}
