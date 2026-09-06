using FintechApp.Requests.ComplaintMaster;
using FintechApp.Responses.ComplaintMaster;

namespace FintechApp.Interfaces.Services
{
    public interface IComplaintMasterService
    {
        Task<List<ComplaintMasterResponse>> GetAll();
        Task<List<ComplaintMasterResponse>> GetByUserID(int userID);
        Task<ComplaintMasterResponse?> GetByID(int id);
        Task<string> Create(CreateComplaintMasterRequest request);
        Task<string> Update(UpdateComplaintMasterRequest request);
        Task<string> Resolve(ResolveComplaintMasterRequest request);
        Task<string> Delete(int id);
    }
}
