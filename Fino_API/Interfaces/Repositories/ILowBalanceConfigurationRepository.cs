using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface ILowBalanceConfigurationRepository
    {
        Task<List<LowBalanceConfiguration>> GetAll();
        Task<List<LowBalanceConfiguration>> GetByUserID(int userID);
        Task<LowBalanceConfiguration?> GetByID(int id);
        Task<int> Insert(LowBalanceConfiguration entity);
        Task<int> Update(LowBalanceConfiguration entity);
        Task<int> Delete(int id);
    }
}
