using FintechApp.Requests.SenderOTP;
using FintechApp.Responses.SenderOTP;
namespace FintechApp.Interfaces.Services
{
    public interface ISenderOTPService
    {
        Task<List<SenderOTPResponse>> GetAll();
        Task<SenderOTPResponse?> GetByID(int id);
        Task<List<SenderOTPResponse>> GetByUserID(int userid);
        Task<string> Create(CreateSenderOTPRequest request);
        Task<string> Update(UpdateSenderOTPRequest request);
        Task<string> Delete(int id);
    }
}
