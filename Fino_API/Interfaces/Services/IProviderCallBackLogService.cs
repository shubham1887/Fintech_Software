using FintechApp.Requests.ProviderCallBackLog;
using FintechApp.Responses.ProviderCallBackLog;
namespace FintechApp.Interfaces.Services
{
    public interface IProviderCallBackLogService
    {
        Task<List<ProviderCallBackLogResponse>> GetAll();
        Task<List<ProviderCallBackLogResponse>> GetByTransaction(int transactionMasterId);
        Task<ProviderCallBackLogResponse?> GetByID(int id);
        Task<string> Create(CreateProviderCallBackLogRequest request);
        Task<string> Update(UpdateProviderCallBackLogRequest request);
        Task<string> Delete(int id);
    }
}
