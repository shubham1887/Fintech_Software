using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.SupportUserAccessDetail;
using FintechApp.Responses.SupportUserAccessDetail;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class SupportUserAccessDetailService : ISupportUserAccessDetailService
    {
        private readonly ISupportUserAccessDetailRepository _repository;
        public SupportUserAccessDetailService(ISupportUserAccessDetailRepository repository) { _repository = repository; }
        private static SupportUserAccessDetailResponse MapToResponse(SupportUserAccessDetail e) => new() { SupportUserAccessDetailID = e.SupportUserAccessDetailID, UserID = e.UserID, Controller = e.Controller, Action = e.Action, MenuGUID = e.MenuGUID, };
        public async Task<List<SupportUserAccessDetailResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<SupportUserAccessDetailResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }
        public async Task<List<SupportUserAccessDetailResponse>> GetByUserID(int userid) { var list = await _repository.GetByUserID(userid); return list.Select(MapToResponse).ToList(); }
        public async Task<string> Create(CreateSupportUserAccessDetailRequest request) { var entity = new SupportUserAccessDetail { UserID = request.UserID, Controller = request.Controller, Action = request.Action, MenuGUID = request.MenuGUID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateSupportUserAccessDetailRequest request) { var entity = await _repository.GetByID(request.SupportUserAccessDetailID); if (entity == null) return Messages.SupportUserAccessDetailMessages.NotFound; entity.UserID = request.UserID; entity.Controller = request.Controller; entity.Action = request.Action; entity.MenuGUID = request.MenuGUID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.SupportUserAccessDetailMessages.NotFound; }
    }
}
