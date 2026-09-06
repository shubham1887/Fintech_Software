using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserServiceSubscriptionChargeRepository
    {
        Task<List<UserServiceSubscriptionCharge>> GetAll();
        Task<UserServiceSubscriptionCharge?> GetByID(int id);
        Task<List<UserServiceSubscriptionCharge>> GetByUserID(int userid);
        Task<int> Insert(UserServiceSubscriptionCharge entity);
        Task<int> Update(UserServiceSubscriptionCharge entity);
        Task<int> Delete(int id);
    }
}
