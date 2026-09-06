using FintechApp.Requests.AuthorizationMaster;
using FintechApp.Responses.AuthorizationMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IAuthorizationMasterService
    {
        Task<List<AuthorizationMasterResponse>> GetAll();
        Task<AuthorizationMasterResponse?> GetByID(int id);

        Task<string> Create(CreateAuthorizationMasterRequest request);
        Task<string> Update(UpdateAuthorizationMasterRequest request);
        Task<string> Delete(int id);
    }
}
