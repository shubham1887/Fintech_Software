using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IAccountOpenSettingRepository
    {
        Task<List<AccountOpenSetting>> GetAll();
        Task<AccountOpenSetting?> GetByID(int id);
        Task<int> Insert(AccountOpenSetting entity);
        Task<int> Update(AccountOpenSetting entity);
        Task<int> Delete(int id);
    }
}
