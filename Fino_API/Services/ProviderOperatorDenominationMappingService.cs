using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.ProviderOperatorDenominationMapping;
using FintechApp.Responses.ProviderOperatorDenominationMapping;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class ProviderOperatorDenominationMappingService : IProviderOperatorDenominationMappingService
    {
        private readonly IProviderOperatorDenominationMappingRepository _repository;
        private readonly IProviderMasterRepository _providerRepository;
        private readonly IOperatorRepository _operatorRepository;
        private readonly IDenominationMasterRepository _denominationRepository;

        public ProviderOperatorDenominationMappingService(
            IProviderOperatorDenominationMappingRepository repository,
            IProviderMasterRepository providerRepository,
            IOperatorRepository operatorRepository,
            IDenominationMasterRepository denominationRepository)
        {
            _repository = repository;
            _providerRepository = providerRepository;
            _operatorRepository = operatorRepository;
            _denominationRepository = denominationRepository;
        }

        private static ProviderOperatorDenominationMappingResponse MapToResponse(ProviderOperatorDenominationMapping e) => new()
        {
            ProviderOperatorDenominationMappingID = e.ProviderOperatorDenominationMappingID,
            ProviderID = e.ProviderID,
            OperatorID = e.OperatorID,
            DenominationMasterID = e.DenominationMasterID,
        };

        // Resolves the ProviderID / OperatorID / DenominationMasterID on each row to display names,
        // so the UI shows "Airtel / Prepaid / 10-5000" instead of raw ids.
        private async Task<List<ProviderOperatorDenominationMappingResponse>> WithNames(List<ProviderOperatorDenominationMapping> list)
        {
            var providers = (await _providerRepository.GetAll()).ToDictionary(p => p.ProviderID, p => p.ProviderName);
            var operators = (await _operatorRepository.GetAllOperators()).ToDictionary(o => o.OperatorID, o => o.OperatorName);
            var denominations = (await _denominationRepository.GetAll()).ToDictionary(d => d.DenominationMasterID, d => d.DenominationRange);

            return list.Select(e =>
            {
                var vm = MapToResponse(e);
                vm.ProviderName = providers.TryGetValue(e.ProviderID, out var pn) ? pn : string.Empty;
                vm.OperatorName = operators.TryGetValue(e.OperatorID, out var on) ? on : string.Empty;
                vm.DenominationRange = denominations.TryGetValue(e.DenominationMasterID, out var dr) ? dr : string.Empty;
                return vm;
            }).ToList();
        }

        public async Task<List<ProviderOperatorDenominationMappingResponse>> GetAll() { var list = await _repository.GetAll(); return await WithNames(list); }
        public async Task<ProviderOperatorDenominationMappingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : (await WithNames(new List<ProviderOperatorDenominationMapping> { e })).First(); }
        public async Task<List<ProviderOperatorDenominationMappingResponse>> GetByProviderID(int providerid) { var list = await _repository.GetByProviderID(providerid); return await WithNames(list); }
        public async Task<string> Create(CreateProviderOperatorDenominationMappingRequest request)
        {
            var entity = new ProviderOperatorDenominationMapping
            {
                ProviderID = request.ProviderID,
                OperatorID = request.OperatorID,
                DenominationMasterID = request.DenominationMasterID,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdateProviderOperatorDenominationMappingRequest request)
        {
            var entity = await _repository.GetByID(request.ProviderOperatorDenominationMappingID);
            if (entity == null) return Messages.ProviderOperatorDenominationMappingMessages.NotFound;
            entity.ProviderID = request.ProviderID;
            entity.OperatorID = request.OperatorID;
            entity.DenominationMasterID = request.DenominationMasterID;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.ProviderOperatorDenominationMappingMessages.NotFound;
        }
    }
}
