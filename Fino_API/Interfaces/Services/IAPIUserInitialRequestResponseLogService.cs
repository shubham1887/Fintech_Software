using FintechApp.Requests.APIUserInitialRequestResponseLog;
using FintechApp.Responses.APIUserInitialRequestResponseLog;
namespace FintechApp.Interfaces.Services
{
    public interface IAPIUserInitialRequestResponseLogService
    {
        Task<List<APIUserInitialRequestResponseLogResponse>> GetAll();
        Task<APIUserInitialRequestResponseLogResponse?> GetByID(int id);
        Task<List<APIUserInitialRequestResponseLogResponse>> GetByUserID(int userID);
        Task<string> Create(CreateAPIUserInitialRequestResponseLogRequest request);
        Task<string> Update(UpdateAPIUserInitialRequestResponseLogRequest request);
        Task<string> Delete(int id);
    }
}
