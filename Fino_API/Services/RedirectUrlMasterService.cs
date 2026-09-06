using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.RedirectUrlMaster;
using FintechApp.Responses.RedirectUrlMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class RedirectUrlMasterService : IRedirectUrlMasterService
    {
        private readonly IRedirectUrlMasterRepository _repository;
        public RedirectUrlMasterService(IRedirectUrlMasterRepository repository) { _repository = repository; }

        private static RedirectUrlMasterResponse MapToResponse(RedirectUrlMaster e) => new()
        {
            RedirectUrlMasterID = e.RedirectUrlMasterID,
            Name = e.Name,
            RedirectUrl = e.RedirectUrl,
            IsActive = e.IsActive,
        };

        public async Task<List<RedirectUrlMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<RedirectUrlMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateRedirectUrlMasterRequest request)
        {
            var entity = new RedirectUrlMaster
            {
                Name = request.Name,
                RedirectUrl = request.RedirectUrl,
                IsActive = request.IsActive,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateRedirectUrlMasterRequest request)
        {
            var entity = await _repository.GetByID(request.RedirectUrlMasterID);
            if (entity == null) return Messages.RedirectUrlMasterMessages.NotFound;
            entity.Name = request.Name;
            entity.RedirectUrl = request.RedirectUrl;
            entity.IsActive = request.IsActive;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.RedirectUrlMasterMessages.NotFound;
        }
    }
}
