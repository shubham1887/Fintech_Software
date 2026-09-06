using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ICashCollectionMasterRepository
    {
        Task<List<CashCollectionMaster>> GetAll();
        Task<CashCollectionMaster?> GetByID(int id);
        Task<List<CashCollectionMaster>> GetByUserID(int userID);
        Task<int> Insert(CashCollectionMaster entity);
        Task<int> Update(CashCollectionMaster entity);
        Task<int> Delete(int id);
    }
}
