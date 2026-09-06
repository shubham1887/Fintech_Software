using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IWrongPasswordHistoryRepository
    {
        Task<List<WrongPasswordHistory>> GetAll();
        Task<WrongPasswordHistory?> GetByID(int id);
        Task<List<WrongPasswordHistory>> GetByUserID(int userid);
        Task<int> Insert(WrongPasswordHistory entity);
        Task<int> Update(WrongPasswordHistory entity);
        Task<int> Delete(int id);
        Task<int> GetCountByUserID(int userId);
        Task<int> DeleteByUserID(int userId);
    }
}
