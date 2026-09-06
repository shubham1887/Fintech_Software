using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserBalanceHistoryRepository
    {
        Task<List<UserBalanceHistory>> GetAll();
        Task<List<UserBalanceHistory>> GetByUserID(int userId);
        Task<UserBalanceHistory?> GetByID(int id);
        Task<int> Insert(UserBalanceHistory entity);
        Task<int> Update(UserBalanceHistory entity);
        Task<int> Delete(int id);
    }
}
