using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UrlTypeMaster;
using FintechApp.Responses.UrlTypeMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UrlTypeMasterService : IUrlTypeMasterService
    {
        private readonly IUrlTypeMasterRepository _repository;
        public UrlTypeMasterService(IUrlTypeMasterRepository repository) { _repository = repository; }
        private static UrlTypeMasterResponse MapToResponse(UrlTypeMaster e) => new() { UrlTypeID = e.UrlTypeID, UrlType = e.UrlType, };
        public async Task<List<UrlTypeMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UrlTypeMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateUrlTypeMasterRequest request) { var entity = new UrlTypeMaster { UrlType = request.UrlType, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUrlTypeMasterRequest request) { var entity = await _repository.GetByID(request.UrlTypeID); if (entity == null) return Messages.UrlTypeMasterMessages.NotFound; entity.UrlType = request.UrlType; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UrlTypeMasterMessages.NotFound; }
    }
}
