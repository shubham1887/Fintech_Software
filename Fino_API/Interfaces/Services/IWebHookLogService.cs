using FintechApp.Requests.WebHookLog;
using FintechApp.Responses.WebHookLog;
namespace FintechApp.Interfaces.Services
{
    public interface IWebHookLogService
    {
        Task<List<WebHookLogResponse>> GetAll();
        Task<WebHookLogResponse?> GetByID(int id);
        Task<string> Create(CreateWebHookLogRequest request);
        Task<string> Update(UpdateWebHookLogRequest request);
        Task<string> Delete(int id);
    }
}
