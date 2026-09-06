using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IOperationalHistoryRepository
    {
        Task<List<OperationalHistory>> GetAll();
        Task<OperationalHistory?> GetByID(int id);
        Task<List<OperationalHistory>> GetByUserID(int userid);
        Task<int> Insert(OperationalHistory entity);
        Task<int> Update(OperationalHistory entity);
        Task<int> Delete(int id);
    }
}
