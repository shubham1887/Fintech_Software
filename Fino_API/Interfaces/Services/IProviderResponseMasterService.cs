using FintechApp.Requests.ProviderResponseMaster;
using FintechApp.Responses.ProviderResponseMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IProviderResponseMasterService
    {
        Task<List<ProviderResponseMasterResponse>> GetAll();
        Task<List<ProviderResponseMasterResponse>> GetByProviderID(int providerId);
        Task<ProviderResponseMasterResponse?> GetByID(int id);
        Task<string> Create(CreateProviderResponseMasterRequest request);
        Task<string> Update(UpdateProviderResponseMasterRequest request);
        Task<string> Delete(int id);
    }
}
