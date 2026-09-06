using FintechApp.Requests.ProviderMaster;
using FintechApp.Responses.ProviderMaster;

namespace FintechApp.Interfaces.Services
{
    public interface IProviderMasterService
    {
        Task<List<ProviderMasterResponse>> GetAll(GetProviderMasterRequest request);
        Task<string> Create(CreateProviderMasterRequest request);
        Task<string> Update(UpdateProviderMasterRequest request);
        Task<string> Delete(DeleteProviderMasterRequest request);
    }
}
