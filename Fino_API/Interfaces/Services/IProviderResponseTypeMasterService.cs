using FintechApp.Requests.ProviderResponseTypeMaster;
using FintechApp.Responses.ProviderResponseTypeMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IProviderResponseTypeMasterService
    {
        Task<List<ProviderResponseTypeMasterResponse>> GetAll();
        Task<ProviderResponseTypeMasterResponse?> GetByID(int id);
        Task<string> Create(CreateProviderResponseTypeMasterRequest request);
        Task<string> Update(UpdateProviderResponseTypeMasterRequest request);
        Task<string> Delete(int id);
    }
}
