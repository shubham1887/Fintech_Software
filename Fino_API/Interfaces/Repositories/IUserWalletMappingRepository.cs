using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserWalletMappingRepository
    {
        Task<List<UserWalletMapping>> GetAll();
        Task<List<UserWalletMapping>> GetByUserID(int userId);
        Task<UserWalletMapping?> GetByID(int id);
        Task<int> Insert(UserWalletMapping entity);
        Task<int> Update(UserWalletMapping entity);
        Task<int> Delete(int id);
    }
}
