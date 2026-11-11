using FintechApp.Requests.APIUserInitialRequestResponseLog;
using FintechApp.Responses.APIUserInitialRequestResponseLog;
namespace FintechApp.Interfaces.Services
{
    public interface IAPIUserInitialRequestResponseLogService
    {
        Task<List<APIUserInitialRequestResponseLogResponse>> GetAll();
        /// <summary>Filtered, paged, with the partner / service / operator names resolved.</summary>
        Task<APIUserLogWrapper> Select(SelectAPIUserLogRequest request);
        Task<APIUserInitialRequestResponseLogResponse?> GetByID(int id);
        Task<List<APIUserInitialRequestResponseLogResponse>> GetByUserID(int userID);
        Task<List<APIUserInitialRequestResponseLogResponse>> GetByTransaction(int transactionMasterID, int callerUserID, int callerUserTypeID);
        Task<string> Create(CreateAPIUserInitialRequestResponseLogRequest request);
        Task<string> Update(UpdateAPIUserInitialRequestResponseLogRequest request);
        Task<string> Delete(int id);
    }
}
