using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IROfferCommissionRepository
    {
        Task<List<ROfferCommission>> GetAll();
        Task<ROfferCommission?> GetByID(int id);
        Task<List<ROfferCommission>> GetByOperatorID(int operatorID);
        Task<int> Insert(ROfferCommission entity);
        Task<int> Update(ROfferCommission entity);
        Task<int> Delete(int id);
    }
}
