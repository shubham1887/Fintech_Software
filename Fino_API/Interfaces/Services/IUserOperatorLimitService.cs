using FintechApp.Requests.UserOperatorLimit;
using FintechApp.Responses.UserOperatorLimit;
namespace FintechApp.Interfaces.Services
{
    public interface IUserOperatorLimitService
    {
        Task<List<UserOperatorLimitResponse>> GetAll();
        Task<List<UserOperatorLimitResponse>> GetByUserID(int userId);
        Task<UserOperatorLimitResponse?> GetByID(int id);
        Task<string> Create(CreateUserOperatorLimitRequest request);
        Task<string> Update(UpdateUserOperatorLimitRequest request);
        Task<string> Delete(int id);
    }
}
