using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ICashCollectionDetailRepository
    {
        Task<List<CashCollectionDetail>> GetAll();
        Task<CashCollectionDetail?> GetByID(int id);
        Task<List<CashCollectionDetail>> GetByUserID(int userID);
        Task<int> Insert(CashCollectionDetail entity);
        Task<int> Update(CashCollectionDetail entity);
        Task<int> Delete(int id);
    }
}
