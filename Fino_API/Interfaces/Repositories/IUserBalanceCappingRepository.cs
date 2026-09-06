using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserBalanceCappingRepository
    {
        Task<List<UserBalanceCapping>> GetAll();
        Task<List<UserBalanceCapping>> GetByUserID(int userId);
        Task<UserBalanceCapping?> GetByID(int id);
        Task<int> Insert(UserBalanceCapping entity);
        Task<int> Update(UserBalanceCapping entity);
        Task<int> Delete(int id);
    }
}
