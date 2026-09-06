using FintechApp.Requests.ProviderOperatorDenominationMapping;
using FintechApp.Responses.ProviderOperatorDenominationMapping;
namespace FintechApp.Interfaces.Services
{
    public interface IProviderOperatorDenominationMappingService
    {
        Task<List<ProviderOperatorDenominationMappingResponse>> GetAll();
        Task<ProviderOperatorDenominationMappingResponse?> GetByID(int id);
        Task<List<ProviderOperatorDenominationMappingResponse>> GetByProviderID(int providerID);
        Task<string> Create(CreateProviderOperatorDenominationMappingRequest request);
        Task<string> Update(UpdateProviderOperatorDenominationMappingRequest request);
        Task<string> Delete(int id);
    }
}
