using FintechApp.Requests.PayoutVendorBeneficiaryMapping;
using FintechApp.Responses.PayoutVendorBeneficiaryMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IPayoutVendorBeneficiaryMappingService
    {
        Task<List<PayoutVendorBeneficiaryMappingResponse>> GetAll();
        Task<PayoutVendorBeneficiaryMappingResponse?> GetByID(int id);
        Task<string> Create(CreatePayoutVendorBeneficiaryMappingRequest request);
        Task<string> Update(UpdatePayoutVendorBeneficiaryMappingRequest request);
        Task<string> Delete(int id);
    }
}
