using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IPaySprintBankRepository
    {
        Task<List<PaySprintBank>> GetAll();
        Task<PaySprintBank?> GetByID(int id);

        Task<int> Insert(PaySprintBank entity);
        Task<int> Update(PaySprintBank entity);
        Task<int> Delete(int id);
    }
}
