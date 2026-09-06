using FintechApp.Requests.ProviderRequestResponseLog;
using FintechApp.Responses.ProviderRequestResponseLog;
namespace FintechApp.Interfaces.Services
{
    public interface IProviderRequestResponseLogService
    {
        Task<List<ProviderRequestResponseLogResponse>> GetAll();
        Task<ProviderRequestResponseLogResponse?> GetByID(int id);
        Task<List<ProviderRequestResponseLogResponse>> GetByUserID(int userID);
        Task<List<ProviderRequestResponseLogResponse>> GetByTransaction(int transactionId);
        Task<string> Create(CreateProviderRequestResponseLogRequest request);
        Task<string> Update(UpdateProviderRequestResponseLogRequest request);
        Task<string> Delete(int id);
    }
}
