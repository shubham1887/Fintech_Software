using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ISubscriptionMasterRepository
    {
        Task<List<SubscriptionMaster>> GetAll();
        Task<SubscriptionMaster?> GetByID(int id);

        Task<int> Insert(SubscriptionMaster entity);
        Task<int> Update(SubscriptionMaster entity);
        Task<int> Delete(int id);
    }
}
