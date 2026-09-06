using FintechApp.Requests.UserCreationWallet;
using FintechApp.Responses.UserCreationWallet;
namespace FintechApp.Interfaces.Services
{
    public interface IUserCreationWalletService
    {
        Task<List<UserCreationWalletResponse>> GetAll();
        Task<List<UserCreationWalletResponse>> GetByUserID(int userId);
        Task<UserCreationWalletResponse?> GetByID(int id);
        Task<string> Create(CreateUserCreationWalletRequest request);
        Task<string> Update(UpdateUserCreationWalletRequest request);
        Task<string> Delete(int id);
    }
}
