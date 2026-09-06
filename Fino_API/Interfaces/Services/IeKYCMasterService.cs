using FintechApp.Requests.eKYCMaster;
using FintechApp.Responses.eKYCMaster;
namespace FintechApp.Interfaces.Services
{
    public interface IeKYCMasterService
    {
        Task<List<eKYCMasterResponse>> GetAll();
        Task<eKYCMasterResponse?> GetByID(int id);
        Task<List<eKYCMasterResponse>> GetByUserID(int userid);
        Task<string> Create(CreateeKYCMasterRequest request);
        Task<string> Update(UpdateeKYCMasterRequest request);
        Task<string> Delete(int id);
    }
}
