using FintechApp.Entities;
namespace FintechApp.Interfaces.Repositories
{
    public interface IWebHookLogRepository
    {
        Task<List<WebHookLog>> GetAll();
        Task<WebHookLog?> GetByID(int id);
        Task<int> Insert(WebHookLog entity);
        Task<int> Update(WebHookLog entity);
        Task<int> Delete(int id);
    }
}
