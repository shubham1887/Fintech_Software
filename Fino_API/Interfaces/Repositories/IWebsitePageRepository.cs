using FintechApp.Entities;

namespace FintechApp.Interfaces.Repositories
{
    public interface IWebsitePageRepository
    {
        Task<List<WebsitePageMaster>> GetAll();
        Task<WebsitePageMaster?> GetByID(int id);
        Task<int> Insert(WebsitePageMaster entity);
        Task<int> Update(WebsitePageMaster entity);
        Task<int> Delete(int id);
    }
}
