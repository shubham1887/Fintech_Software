using FintechApp.Entities;
using FintechApp.Requests.CallMeRequest;
using FintechApp.Responses.CallMeRequest;

namespace FintechApp.Interfaces.Services
{
    public interface ICallMeRequestService
    {
        Task<CallMeRequestResponse?> GetCallMeRequestById(int callMeRequestID);
        Task<List<CallMeRequestResponse>> GetCallMeRequests(GetCallMeRequestRequest request);
        Task<string> CreateCallMeRequest(CreateCallMeRequestRequest request);
        Task<string> UpdateCallMeRequest(UpdateCallMeRequestRequest request);
        Task<string> DeleteCallMeRequest(DeleteCallMeRequestRequest request);
    }
}