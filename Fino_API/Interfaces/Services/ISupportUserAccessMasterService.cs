using FintechApp.Requests.SupportUserAccessMaster;
using FintechApp.Responses.SupportUserAccessMaster;
namespace FintechApp.Interfaces.Services
{
    public interface ISupportUserAccessMasterService
    {
        Task<List<SupportUserAccessMasterResponse>> GetAll();
        Task<SupportUserAccessMasterResponse?> GetByID(int id);
        Task<List<SupportUserAccessMasterResponse>> GetByUserID(int userid);
        Task<string> Create(CreateSupportUserAccessMasterRequest request);
        Task<string> Update(UpdateSupportUserAccessMasterRequest request);
        Task<string> Delete(int id);
    }
}
