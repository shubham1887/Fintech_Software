using FintechApp.Requests.UserBankDetails;
using FintechApp.Responses.UserBankDetails;
namespace FintechApp.Interfaces.Services
{
    public interface IUserBankDetailsService
    {
        Task<List<UserBankDetailsResponse>> GetAll();
        Task<UserBankDetailsResponse?> GetByID(int id);
        Task<List<UserBankDetailsResponse>> GetByUserID(int userID);
        Task<string> Create(CreateUserBankDetailsRequest request);
        Task<string> Update(UpdateUserBankDetailsRequest request);
        Task<string> Delete(int id);
    }
}
