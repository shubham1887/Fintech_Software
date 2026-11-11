using FintechApp.Requests.LoginHistory;
using FintechApp.Responses.LoginHistory;

namespace FintechApp.Interfaces.Services
{
    public interface ILoginHistoryService
    {
        Task<List<LoginHistoryResponse>> GetAll(GetLoginHistoryRequest request);

        // Scoped to the caller: admin/support see everything, an upline its network, everyone else
        // only their own sign-ins.
        Task<List<LoginHistoryResponse>> GetAllForCaller(GetLoginHistoryRequest request, int callerUserID, int callerUserTypeID);
        Task<string> Create(CreateLoginHistoryRequest request);
    }
}
