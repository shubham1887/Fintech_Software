using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.ZoneMaster;
using FintechApp.Responses.ZoneMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ZoneMasterService : IZoneMasterService
    {
        private readonly IZoneMasterRepository _repository;

        public ZoneMasterService(IZoneMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ZoneMasterResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<ZoneMasterResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateZoneMasterRequest request)
        {
            var entity = new ZoneMaster { Name = request.Name };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateZoneMasterRequest request)
        {
            var entity = await _repository.GetByID(request.ZoneMasterID);
            if (entity == null) return ZoneMasterMessages.NotFound;
            entity.Name = request.Name;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : ZoneMasterMessages.NotFound;
        }

        private static ZoneMasterResponse MapToResponse(ZoneMaster e) => new()
        {
            ZoneMasterID = e.ZoneMasterID,
            Name = e.Name
        };
    }
}
