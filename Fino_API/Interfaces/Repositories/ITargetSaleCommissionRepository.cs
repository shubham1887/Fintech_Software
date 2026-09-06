using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ITargetSaleCommissionRepository
    {
        Task<List<TargetSaleCommission>> GetAll();
        Task<TargetSaleCommission?> GetByID(int id);
        Task<List<TargetSaleCommission>> GetByUserID(int userid);
        Task<int> Insert(TargetSaleCommission entity);
        Task<int> Update(TargetSaleCommission entity);
        Task<int> Delete(int id);
    }
}
