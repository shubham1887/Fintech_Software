using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IServiceSubscriptionPlanDetailsRepository
    {
        Task<List<ServiceSubscriptionPlanDetails>> GetAll();
        Task<ServiceSubscriptionPlanDetails?> GetByID(int id);

        Task<int> Insert(ServiceSubscriptionPlanDetails entity);
        Task<int> Update(ServiceSubscriptionPlanDetails entity);
        Task<int> Delete(int id);
    }
}
