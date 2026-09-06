using FintechApp.Requests.ProviderOperatorCircleMaster;
using FintechApp.Responses.ProviderOperatorCircleMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IProviderOperatorCircleMasterService
    {
        Task<List<ProviderOperatorCircleMasterResponse>> GetAll();
        Task<ProviderOperatorCircleMasterResponse?> GetByID(int id);

        Task<string> Create(CreateProviderOperatorCircleMasterRequest request);
        Task<string> Update(UpdateProviderOperatorCircleMasterRequest request);
        Task<string> Delete(int id);
    }
}
