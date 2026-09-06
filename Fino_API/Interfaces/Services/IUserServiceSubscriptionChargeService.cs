using FintechApp.Requests.UserServiceSubscriptionCharge;
using FintechApp.Responses.UserServiceSubscriptionCharge;
namespace FintechApp.Interfaces.Services
{
    public interface IUserServiceSubscriptionChargeService
    {
        Task<List<UserServiceSubscriptionChargeResponse>> GetAll();
        Task<UserServiceSubscriptionChargeResponse?> GetByID(int id);
        Task<List<UserServiceSubscriptionChargeResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUserServiceSubscriptionChargeRequest request);
        Task<string> Update(UpdateUserServiceSubscriptionChargeRequest request);
        Task<string> Delete(int id);
    }
}
