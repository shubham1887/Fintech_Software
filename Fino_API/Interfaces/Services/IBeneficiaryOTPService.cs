using FintechApp.Requests.BeneficiaryOTP;
using FintechApp.Responses.BeneficiaryOTP;
namespace FintechApp.Interfaces.Services
{
    public interface IBeneficiaryOTPService
    {
        Task<List<BeneficiaryOTPResponse>> GetAll();
        Task<BeneficiaryOTPResponse?> GetByID(int id);
        Task<List<BeneficiaryOTPResponse>> GetByUserID(int userid);
        Task<string> Create(CreateBeneficiaryOTPRequest request);
        Task<string> Update(UpdateBeneficiaryOTPRequest request);
        Task<string> Delete(int id);
    }
}
