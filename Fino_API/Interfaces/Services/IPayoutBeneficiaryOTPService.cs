using FintechApp.Requests.PayoutBeneficiaryOTP;
using FintechApp.Responses.PayoutBeneficiaryOTP;
namespace FintechApp.Interfaces.Services
{
    public interface IPayoutBeneficiaryOTPService
    {
        Task<List<PayoutBeneficiaryOTPResponse>> GetAll();
        Task<PayoutBeneficiaryOTPResponse?> GetByID(int id);
        Task<List<PayoutBeneficiaryOTPResponse>> GetByUserID(int userID);
        Task<string> Create(CreatePayoutBeneficiaryOTPRequest request);
        Task<string> Update(UpdatePayoutBeneficiaryOTPRequest request);
        Task<string> Delete(int id);
    }
}
