using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.AndroidSectionMaster;
using FintechApp.Responses.AndroidSectionMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class AndroidSectionMasterService : IAndroidSectionMasterService
    {
        private readonly IAndroidSectionMasterRepository _repository;
        public AndroidSectionMasterService(IAndroidSectionMasterRepository repository) { _repository = repository; }
        private static AndroidSectionMasterResponse MapToResponse(AndroidSectionMaster e) => new() { AndroidSectionMasterID = e.AndroidSectionMasterID, SectionName = e.SectionName, };
        public async Task<List<AndroidSectionMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<AndroidSectionMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateAndroidSectionMasterRequest request) { var entity = new AndroidSectionMaster { SectionName = request.SectionName, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateAndroidSectionMasterRequest request) { var entity = await _repository.GetByID(request.AndroidSectionMasterID); if (entity == null) return Messages.AndroidSectionMasterMessages.NotFound; entity.SectionName = request.SectionName; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.AndroidSectionMasterMessages.NotFound; }
    }
}
