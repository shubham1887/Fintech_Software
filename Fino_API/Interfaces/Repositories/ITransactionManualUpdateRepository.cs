using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ITransactionManualUpdateRepository
    {
        Task<List<TransactionManualUpdate>> GetAll();
        Task<TransactionManualUpdate?> GetByID(int id);

        Task<int> Insert(TransactionManualUpdate entity);
        Task<int> Update(TransactionManualUpdate entity);
        Task<int> Delete(int id);
    }
}
