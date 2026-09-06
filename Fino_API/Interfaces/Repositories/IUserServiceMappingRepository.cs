using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserServiceMappingRepository
    {
        Task<List<UserServiceMapping>> GetAll();
        Task<List<UserServiceMapping>> GetByUserID(int userId);
        Task<UserServiceMapping?> GetByID(int id);
        Task<int> Insert(UserServiceMapping entity);
        Task<int> Update(UserServiceMapping entity);
        Task<int> Delete(int id);
    }
}
