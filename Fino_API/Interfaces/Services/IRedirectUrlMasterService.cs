using FintechApp.Requests.RedirectUrlMaster;
using FintechApp.Responses.RedirectUrlMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IRedirectUrlMasterService
    {
        Task<List<RedirectUrlMasterResponse>> GetAll();
        Task<RedirectUrlMasterResponse?> GetByID(int id);
        Task<string> Create(CreateRedirectUrlMasterRequest request);
        Task<string> Update(UpdateRedirectUrlMasterRequest request);
        Task<string> Delete(int id);
    }
}
