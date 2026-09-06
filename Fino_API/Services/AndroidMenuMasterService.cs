using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.AndroidMenuMaster;
using FintechApp.Responses.AndroidMenuMaster;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class AndroidMenuMasterService : IAndroidMenuMasterService
    {
        private readonly IAndroidMenuMasterRepository _repository;
        public AndroidMenuMasterService(IAndroidMenuMasterRepository repository) { _repository = repository; }
        private static AndroidMenuMasterResponse MapToResponse(AndroidMenuMaster e) => new() { AndroidMenuMasterID = e.AndroidMenuMasterID, MenuName = e.MenuName, Key = e.Key, URL = e.URL, UserTypeID = e.UserTypeID, Sequence = e.Sequence, SectionName = e.SectionName, IsActive = e.IsActive, };
        public async Task<List<AndroidMenuMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<AndroidMenuMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateAndroidMenuMasterRequest request) { var entity = new AndroidMenuMaster { MenuName = request.MenuName, Key = request.Key, URL = request.URL, UserTypeID = request.UserTypeID, Sequence = request.Sequence, SectionName = request.SectionName, IsActive = request.IsActive, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateAndroidMenuMasterRequest request) { var entity = await _repository.GetByID(request.AndroidMenuMasterID); if (entity == null) return Messages.AndroidMenuMasterMessages.NotFound; entity.MenuName = request.MenuName; entity.Key = request.Key; entity.URL = request.URL; entity.UserTypeID = request.UserTypeID; entity.Sequence = request.Sequence; entity.SectionName = request.SectionName; entity.IsActive = request.IsActive; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.AndroidMenuMasterMessages.NotFound; }
    }
}
