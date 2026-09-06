using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IProviderCallBackLogRepository
    {
        Task<List<ProviderCallBackLog>> GetAll();
        Task<List<ProviderCallBackLog>> GetByTransaction(int transactionMasterId);
        Task<ProviderCallBackLog?> GetByID(int id);
        Task<int> Insert(ProviderCallBackLog entity);
        Task<int> Update(ProviderCallBackLog entity);
        Task<int> Delete(int id);
    }
}
