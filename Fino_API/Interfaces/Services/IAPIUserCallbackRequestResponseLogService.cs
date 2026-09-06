using FintechApp.Requests.APIUserCallbackRequestResponseLog;
using FintechApp.Responses.APIUserCallbackRequestResponseLog;
namespace FintechApp.Interfaces.Services
{
    public interface IAPIUserCallbackRequestResponseLogService
    {
        Task<List<APIUserCallbackRequestResponseLogResponse>> GetAll();
        Task<APIUserCallbackRequestResponseLogResponse?> GetByID(int id);
        Task<List<APIUserCallbackRequestResponseLogResponse>> GetByUserID(int userID);
        Task<string> Create(CreateAPIUserCallbackRequestResponseLogRequest request);
        Task<string> Update(UpdateAPIUserCallbackRequestResponseLogRequest request);
        Task<string> Delete(int id);
    }
}
