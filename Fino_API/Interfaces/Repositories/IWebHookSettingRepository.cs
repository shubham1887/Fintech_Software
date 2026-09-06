using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IWebHookSettingRepository
    {
        Task<List<WebHookSetting>> GetAll();
        Task<WebHookSetting?> GetByID(int id);
        Task<int> Insert(WebHookSetting entity);
        Task<int> Update(WebHookSetting entity);
        Task<int> Delete(int id);
    }
}
