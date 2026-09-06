using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IUserSubscriptionRepository
    {
        Task<List<UserSubscription>> GetAll();
        Task<List<UserSubscription>> GetByUserID(int userId);
        Task<int> Insert(UserSubscription entity);
        Task<int> Update(UserSubscription entity);
    }
}
