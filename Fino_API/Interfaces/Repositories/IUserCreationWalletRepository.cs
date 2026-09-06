using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserCreationWalletRepository
    {
        Task<List<UserCreationWallet>> GetAll();
        Task<List<UserCreationWallet>> GetByUserID(int userId);
        Task<UserCreationWallet?> GetByID(int id);
        Task<int> Insert(UserCreationWallet entity);
        Task<int> Update(UserCreationWallet entity);
        Task<int> Delete(int id);
    }
}
