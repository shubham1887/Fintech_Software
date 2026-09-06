using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPasswordChangeHistoryRepository
    {
        Task<List<PasswordChangeHistory>> GetAll();
        Task<List<PasswordChangeHistory>> GetByUserID(int userId);
        Task<PasswordChangeHistory?> GetByID(int id);
        Task<int> Insert(PasswordChangeHistory entity);
        Task<int> Update(PasswordChangeHistory entity);
        Task<int> Delete(int id);
        Task<int> GetCountByUserIDAndType(int userId, string type);
    }
}
