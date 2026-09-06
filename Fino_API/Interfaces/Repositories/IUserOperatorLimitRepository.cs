using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserOperatorLimitRepository
    {
        Task<List<UserOperatorLimit>> GetAll();
        Task<List<UserOperatorLimit>> GetByUserID(int userId);
        Task<UserOperatorLimit?> GetByID(int id);
        Task<int> Insert(UserOperatorLimit entity);
        Task<int> Update(UserOperatorLimit entity);
        Task<int> Delete(int id);
    }
}
