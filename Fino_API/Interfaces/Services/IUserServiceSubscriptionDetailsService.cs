using FintechApp.Requests.UserServiceSubscriptionDetails;
using FintechApp.Responses.UserServiceSubscriptionDetails;
namespace FintechApp.Interfaces.Services
{
    public interface IUserServiceSubscriptionDetailsService
    {
        Task<List<UserServiceSubscriptionDetailsResponse>> GetAll();
        Task<UserServiceSubscriptionDetailsResponse?> GetByID(int id);
        Task<List<UserServiceSubscriptionDetailsResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUserServiceSubscriptionDetailsRequest request);
        Task<string> Update(UpdateUserServiceSubscriptionDetailsRequest request);
        Task<string> Delete(int id);
    }
}
