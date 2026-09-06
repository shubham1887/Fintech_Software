using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserwiseLowBalanceRepository
    {
        Task<List<UserwiseLowBalance>> GetAll();
        Task<List<UserwiseLowBalance>> GetByUserID(int userId);
        Task<UserwiseLowBalance?> GetByID(int id);
        Task<int> Insert(UserwiseLowBalance entity);
        Task<int> Update(UserwiseLowBalance entity);
        Task<int> Delete(int id);
    }
}
