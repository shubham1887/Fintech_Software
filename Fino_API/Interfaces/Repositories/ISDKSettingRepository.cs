using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface ISDKSettingRepository
    {
        Task<List<SDKSetting>> GetAll();
        Task<SDKSetting?> GetByID(int id);

        Task<int> Insert(SDKSetting entity);
        Task<int> Update(SDKSetting entity);
        Task<int> Delete(int id);
    }
}
