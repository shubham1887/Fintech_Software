using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserServiceChargeRepository
    {
        Task<List<UserServiceCharge>> GetAll();
        Task<List<UserServiceCharge>> GetByUserID(int userId);
        Task<UserServiceCharge?> GetByID(int id);
        Task<int> Insert(UserServiceCharge entity);
        Task<int> Update(UserServiceCharge entity);
        Task<int> Delete(int id);
    }
}
