using FintechApp.Requests.BrowserUserAgentMaster;
using FintechApp.Responses.BrowserUserAgentMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IBrowserUserAgentMasterService
    {
        Task<List<BrowserUserAgentMasterResponse>> GetAll();
        Task<BrowserUserAgentMasterResponse?> GetByID(int id);
        Task<string> Create(CreateBrowserUserAgentMasterRequest request);
        Task<string> Update(UpdateBrowserUserAgentMasterRequest request);
        Task<string> Delete(int id);
    }
}
