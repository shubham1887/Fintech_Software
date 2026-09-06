using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IServiceSubscriptionCreatePlanMasterRepository
    {
        Task<List<ServiceSubscriptionCreatePlanMaster>> GetAll();
        Task<ServiceSubscriptionCreatePlanMaster?> GetByID(int id);

        Task<int> Insert(ServiceSubscriptionCreatePlanMaster entity);
        Task<int> Update(ServiceSubscriptionCreatePlanMaster entity);
        Task<int> Delete(int id);
    }
}
