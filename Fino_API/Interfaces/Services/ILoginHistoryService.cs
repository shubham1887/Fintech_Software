using FintechApp.Requests.LoginHistory;
using FintechApp.Responses.LoginHistory;

namespace FintechApp.Interfaces.Services
{
    public interface ILoginHistoryService
    {
        Task<List<LoginHistoryResponse>> GetAll(GetLoginHistoryRequest request);
        Task<string> Create(CreateLoginHistoryRequest request);
    }
}
