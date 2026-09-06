using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.MenuMaster;
using FintechApp.Responses.MenuMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class MenuMasterService : IMenuMasterService
    {
        private readonly IMenuMasterRepository _repository;
        public MenuMasterService(IMenuMasterRepository repository) { _repository = repository; }

        private static MenuMasterResponse MapToResponse(MenuMaster e) => new()
        {
            MenuMasterID = e.MenuMasterID,
            MenuSequence = e.MenuSequence,
            Name = e.Name,
            Url = e.Url,
            Icon = e.Icon,
            Description = e.Description,
            IsAdmin = e.IsAdmin,
            IsSuperDistributer = e.IsSuperDistributer,
            IsDistributer = e.IsDistributer,
            IsRetailer = e.IsRetailer,
            IsAPIUser = e.IsAPIUser,
            IsSupport = e.IsSupport,
            Level = e.Level,
            GUID = e.GUID,
            ParentMenuGUID = e.ParentMenuGUID,
        };

        public async Task<List<MenuMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<MenuMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateMenuMasterRequest request)
        {
            var entity = new MenuMaster
            {
                MenuSequence = request.MenuSequence,
                Name = request.Name,
                Url = request.Url,
                Icon = request.Icon,
                Description = request.Description,
                IsAdmin = request.IsAdmin,
                IsSuperDistributer = request.IsSuperDistributer,
                IsDistributer = request.IsDistributer,
                IsRetailer = request.IsRetailer,
                IsAPIUser = request.IsAPIUser,
                IsSupport = request.IsSupport,
                Level = request.Level,
                GUID = request.GUID,
                ParentMenuGUID = request.ParentMenuGUID,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateMenuMasterRequest request)
        {
            var entity = await _repository.GetByID(request.MenuMasterID);
            if (entity == null) return Messages.MenuMasterMessages.NotFound;
            entity.MenuSequence = request.MenuSequence;
            entity.Name = request.Name;
            entity.Url = request.Url;
            entity.Icon = request.Icon;
            entity.Description = request.Description;
            entity.IsAdmin = request.IsAdmin;
            entity.IsSuperDistributer = request.IsSuperDistributer;
            entity.IsDistributer = request.IsDistributer;
            entity.IsRetailer = request.IsRetailer;
            entity.IsAPIUser = request.IsAPIUser;
            entity.IsSupport = request.IsSupport;
            entity.Level = request.Level;
            entity.GUID = request.GUID;
            entity.ParentMenuGUID = request.ParentMenuGUID;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.MenuMasterMessages.NotFound;
        }
    }
}
