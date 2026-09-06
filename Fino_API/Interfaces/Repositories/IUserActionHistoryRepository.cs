using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserActionHistoryRepository
    {
        Task<List<UserActionHistory>> GetAll();
        Task<UserActionHistory?> GetByID(int id);
        Task<List<UserActionHistory>> GetByUserID(int userid);
        Task<int> Insert(UserActionHistory entity);
        Task<int> Update(UserActionHistory entity);
        Task<int> Delete(int id);
    }
}
