using FintechApp.Requests.CallbackMaster;
using FintechApp.Responses.CallbackMaster;
namespace FintechApp.Interfaces.Services
{
    public interface ICallbackMasterService
    {
        Task<List<CallbackMasterResponse>> GetAll();
        Task<List<CallbackMasterResponse>> GetByUserID(int userId);
        Task<CallbackMasterResponse?> GetByID(int id);
        Task<string> Create(CreateCallbackMasterRequest request);
        Task<string> Update(UpdateCallbackMasterRequest request);
        Task<string> Delete(int id);
    }
}
