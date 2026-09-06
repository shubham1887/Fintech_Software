using FintechApp.Entities;
using FintechApp.Interfaces.Repositories;
using FintechApp.Interfaces.Services;
using FintechApp.Requests.UPIVendorBeneficiaryMapping;
using FintechApp.Responses.UPIVendorBeneficiaryMapping;
using FintechApp.StaticModels;
namespace FintechApp.Services
{
    public class UPIVendorBeneficiaryMappingService : IUPIVendorBeneficiaryMappingService
    {
        private readonly IUPIVendorBeneficiaryMappingRepository _repository;
        public UPIVendorBeneficiaryMappingService(IUPIVendorBeneficiaryMappingRepository repository) { _repository = repository; }
        private static UPIVendorBeneficiaryMappingResponse MapToResponse(UPIVendorBeneficiaryMapping e) => new() { UPIVendorBeneficiaryMappingID = e.UPIVendorBeneficiaryMappingID, ProviderID = e.ProviderID, UPIVendorBeneficiaryID = e.UPIVendorBeneficiaryID, UpiBeneficiaryID = e.UpiBeneficiaryID, };
        public async Task<List<UPIVendorBeneficiaryMappingResponse>> GetAll() { var list = await _repository.GetAll(); return list.Select(MapToResponse).ToList(); }
        public async Task<UPIVendorBeneficiaryMappingResponse?> GetByID(int id) { var e = await _repository.GetByID(id); return e == null ? null : MapToResponse(e); }

        public async Task<string> Create(CreateUPIVendorBeneficiaryMappingRequest request) { var entity = new UPIVendorBeneficiaryMapping { ProviderID = request.ProviderID, UPIVendorBeneficiaryID = request.UPIVendorBeneficiaryID, UpiBeneficiaryID = request.UpiBeneficiaryID, }; await _repository.Insert(entity); return StatusType.Ok; }
        public async Task<string> Update(UpdateUPIVendorBeneficiaryMappingRequest request) { var entity = await _repository.GetByID(request.UPIVendorBeneficiaryMappingID); if (entity == null) return Messages.UPIVendorBeneficiaryMappingMessages.NotFound; entity.ProviderID = request.ProviderID; entity.UPIVendorBeneficiaryID = request.UPIVendorBeneficiaryID; entity.UpiBeneficiaryID = request.UpiBeneficiaryID; await _repository.Update(entity); return StatusType.Ok; }
        public async Task<string> Delete(int id) { int result = await _repository.Delete(id); return result > 0 ? StatusType.Ok : Messages.UPIVendorBeneficiaryMappingMessages.NotFound; }
    }
}
