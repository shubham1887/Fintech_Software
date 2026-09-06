using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ITransactionDetailsRepository
    {
        Task<List<TransactionDetails>> GetAll();
        Task<TransactionDetails?> GetByID(int id);
        Task<List<TransactionDetails>> GetByUserID(int userid);
        Task<int> Insert(TransactionDetails entity);
        Task<int> Update(TransactionDetails entity);
        Task<int> Delete(int id);
    }
}
