using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.ProviderResponseMaster;
using FintechApp.Responses.ProviderResponseMaster;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderResponseMasterService : IProviderResponseMasterService
    {
        private readonly IProviderResponseMasterRepository _repository;

        public ProviderResponseMasterService(IProviderResponseMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProviderResponseMasterResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<ProviderResponseMasterResponse>> GetByProviderID(int providerId)
        {
            var list = await _repository.GetByProviderID(providerId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<ProviderResponseMasterResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateProviderResponseMasterRequest request)
        {
            var entity = new ProviderResponseMaster
            {
                ProviderID = request.ProviderID,
                TextMustexits = request.TextMustexits,
                Type = request.Type,
                Status = request.Status,
                Response = request.Response,
                IsReference = request.IsReference,
                IsActive = request.IsActive,
                DateTime = DateTime.UtcNow,
                UpdatedateTime = DateTime.UtcNow,
                IsDeleted = false
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateProviderResponseMasterRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderResponseMasterID);
            if (entity == null) return ProviderResponseMasterMessages.NotFound;
            entity.ProviderID = request.ProviderID;
            entity.TextMustexits = request.TextMustexits;
            entity.Type = request.Type;
            entity.Status = request.Status;
            entity.Response = request.Response;
            entity.IsReference = request.IsReference;
            entity.IsActive = request.IsActive;
            entity.UpdatedateTime = DateTime.UtcNow;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : ProviderResponseMasterMessages.NotFound;
        }

        private static ProviderResponseMasterResponse MapToResponse(ProviderResponseMaster e) => new()
        {
            ProviderResponseMasterID = e.ProviderResponseMasterID,
            ProviderID = e.ProviderID,
            TextMustexits = e.TextMustexits,
            Type = e.Type,
            Status = e.Status,
            Response = e.Response,
            IsReference = e.IsReference,
            IsActive = e.IsActive,
            DateTime = e.DateTime,
            UpdatedateTime = e.UpdatedateTime
        };
    }
}
