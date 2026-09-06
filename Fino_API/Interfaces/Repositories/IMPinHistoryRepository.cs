using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IMPinHistoryRepository
    {
        Task<List<MPinHistory>> GetAll();
        Task<MPinHistory?> GetByID(int id);
        Task<List<MPinHistory>> GetByUserID(int userid);
        Task<int> Insert(MPinHistory entity);
        Task<int> Update(MPinHistory entity);
        Task<int> Delete(int id);
    }
}
