using FintechApp.Requests.UserWisePlanLimit;
using FintechApp.Responses.UserWisePlanLimit;
namespace FintechApp.Interfaces.Services
{
    public interface IUserWisePlanLimitService
    {
        Task<List<UserWisePlanLimitResponse>> GetAll();
        Task<UserWisePlanLimitResponse?> GetByID(int id);
        Task<List<UserWisePlanLimitResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUserWisePlanLimitRequest request);
        Task<string> Update(UpdateUserWisePlanLimitRequest request);
        Task<string> Delete(int id);
    }
}
