using FintechApp.Entities;

namespace FintechApp.IRepositories
{
    public interface IPayoutSettingRepository
    {
        Task<List<PayoutSetting>> GetAll();
        Task<PayoutSetting?> GetByID(int id);
        Task<int> Insert(PayoutSetting entity);
        Task<int> Update(PayoutSetting entity);
        Task<int> Delete(int id);
    }
}
