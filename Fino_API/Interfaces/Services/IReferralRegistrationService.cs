using FintechApp.Requests.ReferralRegistration;
using FintechApp.Responses.ReferralRegistration;
namespace FintechApp.Interfaces.Services
{
    public interface IReferralRegistrationService
    {
        Task<List<ReferralRegistrationResponse>> GetAll();
        Task<ReferralRegistrationResponse?> GetByID(int id);
        Task<List<ReferralRegistrationResponse>> GetByUserID(int userid);
        Task<string> Create(CreateReferralRegistrationRequest request);
        Task<string> Update(UpdateReferralRegistrationRequest request);
        Task<string> Delete(int id);
    }
}
