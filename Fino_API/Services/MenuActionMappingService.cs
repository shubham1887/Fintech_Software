using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.MenuActionMapping;
using FintechApp.Responses.MenuActionMapping;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class MenuActionMappingService : IMenuActionMappingService
    {
        private readonly IMenuActionMappingRepository _repository;
        public MenuActionMappingService(IMenuActionMappingRepository repository) { _repository = repository; }
        private static MenuActionMappingResponse MapToResponse(MenuActionMapping e) => new() { MenuActionMappingID = e.MenuActionMappingID, Controller = e.Controller, Action = e.Action, Description = e.Description, MenuMasterGUID = e.MenuMasterGUID, };
        public async Task<List<MenuActionMappingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<MenuActionMappingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateMenuActionMappingRequest request) { var entity = new MenuActionMapping { Controller = request.Controller, Action = request.Action, Description = request.Description, MenuMasterGUID = request.MenuMasterGUID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateMenuActionMappingRequest request) { var entity = await _repository.GetByID(request.MenuActionMappingID); if (entity == null) return Messages.MenuActionMappingMessages.NotFound; entity.Controller = request.Controller; entity.Action = request.Action; entity.Description = request.Description; entity.MenuMasterGUID = request.MenuMasterGUID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.MenuActionMappingMessages.NotFound; }
    }
}
