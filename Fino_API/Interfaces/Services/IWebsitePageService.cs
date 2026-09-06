using FintechApp.Requests.WebsitePage;
using FintechApp.Responses.WebsitePage;

namespace FintechApp.Interfaces.Services
{
    public interface IWebsitePageService
    {
        Task<List<WebsitePageResponse>> GetAll();
        Task<List<WebsitePageResponse>> GetPublished();
        Task<WebsitePageResponse?> GetByID(int id);
        Task<string> Create(CreateWebsitePageRequest request);
        Task<string> Update(UpdateWebsitePageRequest request);
        Task<string> Delete(int id);
    }
}
