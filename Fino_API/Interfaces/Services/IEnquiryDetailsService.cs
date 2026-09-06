using FintechApp.Requests.EnquiryDetails;
using FintechApp.Responses.EnquiryDetails;
namespace FintechApp.Interfaces.Services
{
    public interface IEnquiryDetailsService
    {
        Task<List<EnquiryDetailsResponse>> GetAll();
        Task<EnquiryDetailsResponse?> GetByID(int id);
        Task<string> Create(CreateEnquiryDetailsRequest request);
        Task<string> Update(UpdateEnquiryDetailsRequest request);
        Task<string> Delete(int id);
    }
}
