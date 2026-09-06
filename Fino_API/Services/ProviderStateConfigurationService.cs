using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Messages;
using FintechApp.Requests.ProviderStateConfiguration;
using FintechApp.Responses.ProviderStateConfiguration;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderStateConfigurationService : IProviderStateConfigurationService
    {
        private readonly IProviderStateConfigurationRepository _repository;

        public ProviderStateConfigurationService(IProviderStateConfigurationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProviderStateConfigurationResponse>> GetAll()
        {
            var list = await _repository.GetAll();
            return list.Select(MapToResponse).ToList();
        }

        public async Task<List<ProviderStateConfigurationResponse>> GetByProviderID(int providerId)
        {
            var list = await _repository.GetByProviderID(providerId);
            return list.Select(MapToResponse).ToList();
        }

        public async Task<ProviderStateConfigurationResponse?> GetByID(int id)
        {
            var entity = await _repository.GetByID(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<string> Create(CreateProviderStateConfigurationRequest request)
        {
            var entity = new ProviderStateConfiguration
            {
                ProviderID = request.ProviderID,
                StateID = request.StateID,
                StateCode = request.StateCode
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateProviderStateConfigurationRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderStateConfigurationID);
            if (entity == null) return ProviderStateConfigurationMessages.NotFound;
            entity.ProviderID = request.ProviderID;
            entity.StateID = request.StateID;
            entity.StateCode = request.StateCode;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : ProviderStateConfigurationMessages.NotFound;
        }

        private static ProviderStateConfigurationResponse MapToResponse(ProviderStateConfiguration e) => new()
        {
            ProviderStateConfigurationID = e.ProviderStateConfigurationID,
            ProviderID = e.ProviderID,
            StateID = e.StateID,
            StateCode = e.StateCode
        };
    }
}
