using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderResponseTypeMaster;
using FintechApp.Responses.ProviderResponseTypeMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderResponseTypeMasterService : IProviderResponseTypeMasterService
    {
        private readonly IProviderResponseTypeMasterRepository _repository;
        public ProviderResponseTypeMasterService(IProviderResponseTypeMasterRepository repository) { _repository = repository; }

        private static ProviderResponseTypeMasterResponse MapToResponse(ProviderResponseTypeMaster e) => new()
        {
            ProviderResponseTypeMasterID = e.ProviderResponseTypeMasterID,
            Type = e.Type,
        };

        public async Task<List<ProviderResponseTypeMasterResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<ProviderResponseTypeMasterResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateProviderResponseTypeMasterRequest request)
        {
            var entity = new ProviderResponseTypeMaster
            {
                Type = request.Type,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateProviderResponseTypeMasterRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderResponseTypeMasterID);
            if (entity == null) return Messages.ProviderResponseTypeMasterMessages.NotFound;
            entity.Type = request.Type;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ProviderResponseTypeMasterMessages.NotFound;
        }
    }
}
