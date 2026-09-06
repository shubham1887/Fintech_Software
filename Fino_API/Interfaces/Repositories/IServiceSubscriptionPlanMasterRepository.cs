using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IServiceSubscriptionPlanMasterRepository
    {
        Task<List<ServiceSubscriptionPlanMaster>> GetAll();
        Task<ServiceSubscriptionPlanMaster?> GetByID(int id);
        Task<int> Insert(ServiceSubscriptionPlanMaster entity);
        Task<int> Update(ServiceSubscriptionPlanMaster entity);
        Task<int> Delete(int id);
    }
}
