using FintechApp.Requests.CallMeCategoryMaster;

namespace FintechApp.Interfaces.Repositories
{
    public interface ICallMeCategoryMasterRepository
    {
        Task<List<CallMeCategoryMaster>> GetAllCallMeCategoryMasters();
        Task<CallMeCategoryMaster?> GetCallMeCategoryMasterById(int callMeCategoryMasterID);
        Task<List<CallMeCategoryMaster>> GetCallMeCategoryMasters(GetCallMeCategoryMasterRequest request);
        Task<int> Insert(CallMeCategoryMaster callMeCategoryMaster);
        Task<int> Update(CallMeCategoryMaster callMeCategoryMaster);
        Task<int> Delete(CallMeCategoryMaster callMeCategoryMaster);
    }
}