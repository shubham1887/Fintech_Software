using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.PayoutVendorBeneficiaryMapping;
using FintechApp.Responses.PayoutVendorBeneficiaryMapping;
using FintechApp.StaticModels;

namespace FintechApp.Services
{
    public class PayoutVendorBeneficiaryMappingService : IPayoutVendorBeneficiaryMappingService
    {
        private readonly IPayoutVendorBeneficiaryMappingRepository _repository;
        public PayoutVendorBeneficiaryMappingService(IPayoutVendorBeneficiaryMappingRepository repository) { _repository = repository; }

        private static PayoutVendorBeneficiaryMappingResponse MapToResponse(PayoutVendorBeneficiaryMapping e) => new()
        {
            PayoutVendorBeneficiaryMappingID = e.PayoutVendorBeneficiaryMappingID,
            ProviderID = e.ProviderID,
            PayoutVendorBeneficiaryID = e.PayoutVendorBeneficiaryID,
            PayoutBeneficaryID = e.PayoutBeneficaryID,
        };

        public async Task<List<PayoutVendorBeneficiaryMappingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<PayoutVendorBeneficiaryMappingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreatePayoutVendorBeneficiaryMappingRequest request)
        {
            var entity = new PayoutVendorBeneficiaryMapping
            {
                ProviderID = request.ProviderID,
                PayoutVendorBeneficiaryID = request.PayoutVendorBeneficiaryID,
                PayoutBeneficaryID = request.PayoutBeneficaryID,
            };
            await _repository.Insert(entity);
            return StatusType.Ok;
        }

        public async Task<string> Update(UpdatePayoutVendorBeneficiaryMappingRequest request)
        {
            var entity = await _repository.GetByID(request.PayoutVendorBeneficiaryMappingID);
            if (entity == null) return Messages.PayoutVendorBeneficiaryMappingMessages.NotFound;
            entity.ProviderID = request.ProviderID;
            entity.PayoutVendorBeneficiaryID = request.PayoutVendorBeneficiaryID;
            entity.PayoutBeneficaryID = request.PayoutBeneficaryID;
            await _repository.Update(entity);
            return StatusType.Ok;
        }

        public async Task<string> Delete(int id)
        {
            int result = await _repository.Delete(id);
            return result > 0 ? StatusType.Ok : Messages.PayoutVendorBeneficiaryMappingMessages.NotFound;
        }
    }
}
