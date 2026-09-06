using FintechApp.Requests.eKYCLog;
using FintechApp.Responses.eKYCLog;
namespace FintechApp.Interfaces.Services
{
    public interface IeKYCLogService
    {
        Task<List<eKYCLogResponse>> GetAll();
        Task<eKYCLogResponse?> GetByID(int id);
        Task<List<eKYCLogResponse>> GetByUserID(int userid);
        Task<string> Create(CreateeKYCLogRequest request);
        Task<string> Update(UpdateeKYCLogRequest request);
        Task<string> Delete(int id);
    }
}
