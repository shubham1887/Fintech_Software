using FintechApp.Requests.UserPlanUtilization;
using FintechApp.Responses.UserPlanUtilization;
namespace FintechApp.Interfaces.Services
{
    public interface IUserPlanUtilizationService
    {
        Task<List<UserPlanUtilizationResponse>> GetAll();
        Task<UserPlanUtilizationResponse?> GetByID(int id);
        Task<List<UserPlanUtilizationResponse>> GetByUserID(int userid);
        Task<string> Create(CreateUserPlanUtilizationRequest request);
        Task<string> Update(UpdateUserPlanUtilizationRequest request);
        Task<string> Delete(int id);
    }
}
