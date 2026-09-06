using FintechApp.Requests.UserWalletMaster;
using FintechApp.Responses.UserWalletMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IUserWalletMasterService
    {
        Task<List<UserWalletMasterResponse>> GetAll();
        Task<List<UserWalletMasterResponse>> GetByUserID(int userId);
        Task<UserWalletMasterResponse?> GetByID(int id);
        Task<string> Create(CreateUserWalletMasterRequest request);
        Task<string> Update(UpdateUserWalletMasterRequest request);
        Task<string> Delete(int id);
    }
}
