using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ICashDepositSettingRepository
    {
        Task<List<CashDepositSetting>> GetAll();
        Task<CashDepositSetting?> GetByID(int id);
        Task<int> Insert(CashDepositSetting entity);
        Task<int> Update(CashDepositSetting entity);
        Task<int> Delete(int id);
    }
}
