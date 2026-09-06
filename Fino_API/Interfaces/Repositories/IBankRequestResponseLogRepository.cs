using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IBankRequestResponseLogRepository
    {
        Task<List<BankRequestResponseLog>> GetAll();
        Task<BankRequestResponseLog?> GetByID(int id);
        Task<List<BankRequestResponseLog>> GetByUserID(int userid);
        Task<int> Insert(BankRequestResponseLog entity);
        Task<int> Update(BankRequestResponseLog entity);
        Task<int> Delete(int id);
    }
}
