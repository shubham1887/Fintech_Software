using FintechApp.Requests.LastChangeUserDetails;
using FintechApp.Responses.LastChangeUserDetails;
namespace FintechApp.Interfaces.Services
{
    public interface ILastChangeUserDetailsService
    {
        Task<List<LastChangeUserDetailsResponse>> GetAll();
        Task<LastChangeUserDetailsResponse?> GetByID(int id);

        Task<string> Create(CreateLastChangeUserDetailsRequest request);
        Task<string> Update(UpdateLastChangeUserDetailsRequest request);
        Task<string> Delete(int id);
    }
}
