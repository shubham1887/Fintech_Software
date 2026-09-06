using FintechApp.Requests.UserCreationMaster;
using FintechApp.Responses.UserCreationMaster;

namespace FintechApp.Interfaces.Services
{
    public interface IUserCreationMasterService
    {
        Task<List<UserCreationMasterResponse>> GetAll();
        Task<UserCreationMasterResponse?> GetByID(int id);
        Task<List<UserCreationMasterResponse>> GetByUserID(int userID);
        Task<string> Create(CreateUserCreationMasterRequest request);
        Task<string> Update(UpdateUserCreationMasterRequest request);
        Task<string> Delete(int id);
    }
}
