using FintechApp.Requests.ProviderRequestResponseLog;
using FintechApp.Responses.ProviderRequestResponseLog;
namespace FintechApp.Interfaces.Services
{
    public interface IProviderRequestResponseLogService
    {
        Task<List<ProviderRequestResponseLogResponse>> GetAll();

        // Filtered + paged log for the Provider Log screen (date range, provider, service, entity,
        // call type, free text) — replaces pulling the whole table down to filter it in the browser.
        Task<ProviderRequestResponseLogPage> Search(SearchProviderRequestResponseLogRequest request);
        Task<ProviderRequestResponseLogResponse?> GetByID(int id);
        Task<List<ProviderRequestResponseLogResponse>> GetByUserID(int userID);
        Task<List<ProviderRequestResponseLogResponse>> GetByTransaction(int transactionId, int callerUserTypeID);
        Task<string> Create(CreateProviderRequestResponseLogRequest request);
        Task<string> Update(UpdateProviderRequestResponseLogRequest request);
        Task<string> Delete(int id);
    }
}
