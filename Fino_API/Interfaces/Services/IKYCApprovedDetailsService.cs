using FintechApp.Requests.KYCApprovedDetails;
using FintechApp.Responses.KYCApprovedDetails;
namespace FintechApp.Interfaces.Services
{
    public interface IKYCApprovedDetailsService
    {
        Task<List<KYCApprovedDetailsResponse>> GetAll();
        Task<KYCApprovedDetailsResponse?> GetByID(int id);
        Task<List<KYCApprovedDetailsResponse>> GetByUserID(int userID);
        Task<string> Create(CreateKYCApprovedDetailsRequest request);
        Task<string> Update(UpdateKYCApprovedDetailsRequest request);
        Task<string> Delete(int id);
    }
}
