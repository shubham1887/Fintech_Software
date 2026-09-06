using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ITransactionStatusChangeRepository
    {
        Task<List<TransactionStatusChange>> GetAll();
        Task<TransactionStatusChange?> GetByID(int id);
        Task<List<TransactionStatusChange>> GetByUserID(int userid);
        Task<int> Insert(TransactionStatusChange entity);
        Task<int> Update(TransactionStatusChange entity);
        Task<int> Delete(int id);
    }
}
