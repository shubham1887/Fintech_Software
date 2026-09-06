using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IServiceAndUserwiseSubscriptionRepository
    {
        Task<List<ServiceAndUserwiseSubscription>> GetAll();
        Task<ServiceAndUserwiseSubscription?> GetByID(int id);
        Task<List<ServiceAndUserwiseSubscription>> GetByUserID(int userID);
        Task<int> Insert(ServiceAndUserwiseSubscription entity);
        Task<int> Update(ServiceAndUserwiseSubscription entity);
        Task<int> Delete(int id);
    }
}
