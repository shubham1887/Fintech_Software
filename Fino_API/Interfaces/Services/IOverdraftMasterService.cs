using FintechApp.Requests.OverdraftMaster;
using FintechApp.Responses.OverdraftMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IOverdraftMasterService
    {
        Task<List<OverdraftMasterResponse>> GetAll();
        Task<OverdraftMasterResponse?> GetByID(int id);
        Task<List<OverdraftMasterResponse>> GetByUserID(int userid);
        Task<string> Create(CreateOverdraftMasterRequest request);
        Task<string> Update(UpdateOverdraftMasterRequest request);
        Task<string> Delete(int id);
    }
}
