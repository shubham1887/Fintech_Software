using FintechApp.Requests.UPIVendorBeneficiaryMapping;
using FintechApp.Responses.UPIVendorBeneficiaryMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IUPIVendorBeneficiaryMappingService
    {
        Task<List<UPIVendorBeneficiaryMappingResponse>> GetAll();
        Task<UPIVendorBeneficiaryMappingResponse?> GetByID(int id);

        Task<string> Create(CreateUPIVendorBeneficiaryMappingRequest request);
        Task<string> Update(UpdateUPIVendorBeneficiaryMappingRequest request);
        Task<string> Delete(int id);
    }
}
