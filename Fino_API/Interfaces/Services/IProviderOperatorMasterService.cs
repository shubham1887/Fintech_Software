using FintechApp.Requests.ProviderOperatorMaster;
using FintechApp.Responses.ProviderOperatorMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IProviderOperatorMasterService
    {
        Task<List<ProviderOperatorMasterResponse>> GetAll();
        Task<List<ProviderOperatorMasterResponse>> GetByProviderID(int providerId);
        Task<ProviderOperatorMasterResponse?> GetByID(int id);
        Task<string> Create(CreateProviderOperatorMasterRequest request);
        Task<string> Update(UpdateProviderOperatorMasterRequest request);
        Task<string> Delete(int id);
    }
}
