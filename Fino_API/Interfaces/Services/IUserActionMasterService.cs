using FintechApp.Requests.UserActionMaster;
using FintechApp.Responses.UserActionMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IUserActionMasterService
    {
        Task<List<UserActionMasterResponse>> GetAll();
        Task<UserActionMasterResponse?> GetByID(int id);
        Task<List<UserActionMasterResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUserActionMasterRequest request);
        Task<string> Update(UpdateUserActionMasterRequest request);
        Task<string> Delete(int id);
    }
}
