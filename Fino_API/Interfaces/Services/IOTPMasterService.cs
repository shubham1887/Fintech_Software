using FintechApp.Requests.OTPMaster;
using FintechApp.Responses.OTPMaster;

namespace FintechApp.Interfaces.Services
{
    public interface IOTPMasterService
    {
        Task<List<OTPMasterResponse>> GetAll();
        Task<List<OTPMasterResponse>> GetByUserID(int userID);
        Task<OTPMasterResponse?> GetByID(int id);
        Task<string> Create(CreateOTPMasterRequest request);
        Task<string> Update(UpdateOTPMasterRequest request);
        Task<string> Delete(int id);
    }
}
