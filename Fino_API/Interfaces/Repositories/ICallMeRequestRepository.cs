using FintechApp.Entities;
using FintechApp.Requests.CallMeRequest;

namespace FintechApp.Interfaces.Repositories
{
    public interface ICallMeRequestRepository
    {
        Task<List<CallMeRequest>> GetAllCallMeRequests();
        Task<CallMeRequest?> GetCallMeRequestById(int callMeRequestID);
        Task<List<CallMeRequest>> GetCallMeRequests(GetCallMeRequestRequest request);
        Task<int> Insert(CallMeRequest callMeRequest);
        Task<int> Update(CallMeRequest callMeRequest);
        Task<int> Delete(CallMeRequest callMeRequest);
    }
}