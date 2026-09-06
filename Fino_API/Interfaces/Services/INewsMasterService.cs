using FintechApp.Requests.News;
using FintechApp.Responses.News;

namespace FintechApp.Interfaces.Services
{
    public interface INewsMasterService
    {
        Task<List<NewsResponse>> GetAll();
        Task<NewsResponse?> GetByID(int newsId);
        Task<string> Create(CreateNewsRequest request);
        Task<string> Update(UpdateNewsRequest request);
        Task<string> Delete(int newsId);
    }
}
