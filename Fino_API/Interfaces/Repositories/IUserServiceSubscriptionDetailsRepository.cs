using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IUserServiceSubscriptionDetailsRepository
    {
        Task<List<UserServiceSubscriptionDetails>> GetAll();
        Task<UserServiceSubscriptionDetails?> GetByID(int id);
        Task<List<UserServiceSubscriptionDetails>> GetByUserID(int userid);
        Task<int> Insert(UserServiceSubscriptionDetails entity);
        Task<int> Update(UserServiceSubscriptionDetails entity);
        Task<int> Delete(int id);
    }
}
