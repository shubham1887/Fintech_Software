using FintechApp.Requests.CallMeCategoryMaster;
using FintechApp.Responses.CallMeCategoryMaster;

namespace FintechApp.Interfaces.Services
{
    public interface ICallMeCategoryMasterService
    {
        Task<CallMeCategoryMasterResponse?> GetCallMeCategoryMasterById(int callMeCategoryMasterID);
        Task<List<CallMeCategoryMasterResponse>> GetCallMeCategoryMasters(GetCallMeCategoryMasterRequest request);
        Task<string> CreateCallMeCategoryMaster(CreateCallMeCategoryMasterRequest request);
        Task<string> UpdateCallMeCategoryMaster(UpdateCallMeCategoryMasterRequest request);
        Task<string> DeleteCallMeCategoryMaster(DeleteCallMeCategoryMasterRequest request);
    }
}