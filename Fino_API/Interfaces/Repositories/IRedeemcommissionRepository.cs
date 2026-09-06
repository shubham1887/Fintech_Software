using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IRedeemcommissionRepository
    {
        Task<List<Redeemcommission>> GetAll();
        Task<Redeemcommission?> GetByID(int id);
        Task<List<Redeemcommission>> GetByUserID(int userID);
        Task<int> Insert(Redeemcommission entity);
        Task<int> Update(Redeemcommission entity);
        Task<int> Delete(int id);
    }
}
