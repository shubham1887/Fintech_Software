using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ITransactionProviderHistoryRepository
    {
        Task<List<TransactionProviderHistory>> GetAll();
        Task<TransactionProviderHistory?> GetByID(int id);

        Task<int> Insert(TransactionProviderHistory entity);
        Task<int> Update(TransactionProviderHistory entity);
        Task<int> Delete(int id);
    }
}
